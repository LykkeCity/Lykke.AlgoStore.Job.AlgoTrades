using System;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Domain;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.AlgoStore.Service.AlgoTrades.Infrastructure.Strings;
using Lykke.AlgoStore.Service.AlgoTrades.Validations;
using Lykke.Common.Api.Contract.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;

namespace Lykke.AlgoStore.Service.AlgoTrades.Controllers
{
    [Route("api/algoInstanceTrades")]
    public class AlgoInstanceTradesController : Controller
    {
        private IAlgoInstanceTradeRepository _algoInstanceTradeRepository;
        private IAlgoInstanceTradesHistoryService _algoInstanceTradesHistoryService;
        private ILog _log;

        public AlgoInstanceTradesController(IAlgoInstanceTradeRepository algoInstanceTradeRepository,
            IAlgoInstanceTradesHistoryService algoInstanceTradesHistoryService, ILogFactory logFactory)
        {
            _algoInstanceTradeRepository = algoInstanceTradeRepository;
            _algoInstanceTradesHistoryService = algoInstanceTradesHistoryService;
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        ///Returns Algo Instance trades
        /// </summary>
        [HttpGet]
        [SwaggerOperation("GetAlgoInstanceTrades")]
        [ProducesResponseType(typeof(IEnumerable<AlgoInstanceTradeResponseModel>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] string instanceId, [FromQuery] string tradedAssetId,
            [FromQuery] int maxNumberToFetch)
        {
            if (!ParametersValidator.ValidateMaxNumberOfAlgoInstances(maxNumberToFetch))
                return BadRequest(ErrorResponse.Create(Phrases.MaxCountOfItems));

            var records =
                await _algoInstanceTradeRepository.GetAlgoInstaceTradesByTradedAssetAsync(instanceId, tradedAssetId,
                    maxNumberToFetch);

            var result = await Task.WhenAll(records.Select(x => _algoInstanceTradesHistoryService.ExecuteAsync(x)));

            return Ok(result);
        }

        /// <summary>
        ///Returns Algo Instance trades
        /// </summary>
        [HttpGet("period")]
        [SwaggerOperation("GetAlgoInstanceTradesByPeriod")]
        [ProducesResponseType(typeof(IEnumerable<AlgoInstanceTradeResponseModel>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAlgoInstanceTradesByPeriod([FromQuery] string instanceId,
            [FromQuery] string tradedAssetId, [FromQuery] DateTime fromMoment, [FromQuery] DateTime toMoment)
        {
            _log.Info(nameof(GetAlgoInstanceTradesByPeriod), $"Get trades from {fromMoment} to {toMoment} ",
                instanceId);

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                ModelState.AddModelError(nameof(instanceId), "Must not be empty.");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(tradedAssetId))
            {
                ModelState.AddModelError(nameof(tradedAssetId), "Must not be empty.");
                return BadRequest(ModelState);
            }

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                try
                {
                    var records = (await _algoInstanceTradeRepository.GetInstaceTradesByTradedAssetAndPeriodAsync(instanceId,
                        tradedAssetId, fromMoment.ToUniversalTime(), toMoment.ToUniversalTime(), cts.Token))
                        .ToList();

                    _log.Info(nameof(GetAlgoInstanceTradesByPeriod),
                        $"Found {records.Count} trades from {fromMoment} to {toMoment} ", instanceId);

                    var result = await Task.WhenAll(records.Select(x => _algoInstanceTradesHistoryService.ExecuteAsync(x)));

                    return Ok(result);
                }
                catch (TaskCanceledException)
                {
                    var errorMsg =
                        "Couldn't complete the request withing the time allowed, possibly due to high number of trades.";

                    _log.Warning(nameof(GetAlgoInstanceTradesByPeriod),
                        $"Timeout. Request for Get trades from {fromMoment} to {toMoment}. {errorMsg} ",
                        context: instanceId);

                    return BadRequest(new ErrorResponse().AddModelError("RequestTimeOut", errorMsg));
                }
                catch (Exception ex)
                {
                    _log.Error(nameof(GetAlgoInstanceTradesByPeriod), ex,
                        $"Timeout. Request for Get trades from {fromMoment} to {toMoment}.", instanceId);

                    return StatusCode((int)HttpStatusCode.InternalServerError);
                }
            }
        }
    }
}
