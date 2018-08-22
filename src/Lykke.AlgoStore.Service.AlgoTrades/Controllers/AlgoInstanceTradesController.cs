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

namespace Lykke.AlgoStore.Service.AlgoTrades.Controllers
{
    [Route("api/algoInstanceTrades")]
    public class AlgoInstanceTradesController : Controller
    {
        private IAlgoInstanceTradeRepository _algoInstanceTradeRepository;
        private IAlgoInstanceTradesHistoryService _algoInstanceTradesHistoryService;
        private ILog _log;

        public AlgoInstanceTradesController(IAlgoInstanceTradeRepository algoInstanceTradeRepository,
            IAlgoInstanceTradesHistoryService algoInstanceTradesHistoryService, ILog log)
        {
            _algoInstanceTradeRepository = algoInstanceTradeRepository;
            _algoInstanceTradesHistoryService = algoInstanceTradesHistoryService;
            _log = log;
        }

        /// <summary>
        ///Returns Algo Instance trades
        /// </summary>
        [HttpGet]
        [SwaggerOperation("GetAlgoInstanceTrades")]
        [ProducesResponseType(typeof(IEnumerable<AlgoInstanceTradeResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] string instanceId, [FromQuery]string tradedAssetId, [FromQuery]int maxNumberToFetch)
        {
            if (!ParametersValidator.ValidateMaxNumberOfAlgoInstances(maxNumberToFetch))
                return BadRequest(ErrorResponse.Create(Phrases.MaxCountOfItems));

            var records = await _algoInstanceTradeRepository.GetAlgoInstaceTradesByTradedAssetAsync(instanceId, tradedAssetId, maxNumberToFetch);

            var result = await Task.WhenAll(records.Select(x => _algoInstanceTradesHistoryService.ExecuteAsync(x)));

            return Ok(result);
        }

        /// <summary>
        ///Returns Algo Instance trades
        /// </summary>
        [HttpGet("period")]
        [SwaggerOperation("GetAlgoInstanceTradesByPeriod")]
        [ProducesResponseType(typeof(IEnumerable<AlgoInstanceTradeResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAlgoInstanceTradesByPeriod([FromQuery]string instanceId, [FromQuery]string tradedAssetId, [FromQuery]DateTime fromMoment, [FromQuery]DateTime toMoment)
        {
            await _log.WriteInfoAsync(nameof(GetAlgoInstanceTradesByPeriod), instanceId, $"Get trades from {fromMoment} to {toMoment} ");

            if (String.IsNullOrWhiteSpace(instanceId))
            {
                ModelState.AddModelError(nameof(instanceId), "Must not be empty.");
                return BadRequest(ModelState);
            }
            if (String.IsNullOrWhiteSpace(tradedAssetId))
            {
                ModelState.AddModelError(nameof(tradedAssetId), "Must not be empty.");
                return BadRequest(ModelState);
            }

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                var records = await _algoInstanceTradeRepository.GetInstaceTradesByTradedAssetAndPeriodAsync(instanceId, tradedAssetId, fromMoment.ToUniversalTime(), toMoment.ToUniversalTime(), cts.Token);

                await _log.WriteInfoAsync(nameof(GetAlgoInstanceTradesByPeriod), instanceId, $"Found {records.Count()} trades from {fromMoment} to {toMoment} ");

                var result = await Task.WhenAll(records.Select(x => _algoInstanceTradesHistoryService.ExecuteAsync(x))); 

                return Ok(result);
            } 
            catch (TaskCanceledException)
            {
                var errorMsg = "Couldn't complete the request withing the time allowed, possibly due to high number of trades.";
                await _log.WriteWarningAsync(nameof(GetAlgoInstanceTradesByPeriod), instanceId, $"Timout. Request for Get trades from {fromMoment} to {toMoment}. {errorMsg} ");
                return BadRequest(new ErrorResponse().AddModelError("RequestTimeOut", errorMsg)); 
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(GetAlgoInstanceTradesByPeriod), instanceId, $"Timout. Request for Get trades from {fromMoment} to {toMoment}.", ex);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
