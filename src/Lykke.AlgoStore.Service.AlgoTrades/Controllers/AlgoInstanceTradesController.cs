using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Domain;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.AlgoStore.Service.AlgoTrades.Infrastructure.Strings;
using Lykke.AlgoStore.Service.AlgoTrades.Validations;
using Lykke.Common.Api.Contract.Responses;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.AlgoStore.Service.AlgoTrades.Controllers
{
    [Route("api/algoInstanceTrades")]
    public class AlgoInstanceTradesController : Controller
    {
        private IAlgoInstanceTradeRepository _algoInstanceTradeRepository;
        private IAlgoInstanceTradesHistoryService _algoInstanceTradesHistoryService;
        public AlgoInstanceTradesController(IAlgoInstanceTradeRepository algoInstanceTradeRepository,

            IAlgoInstanceTradesHistoryService algoInstanceTradesHistoryService)
        {
            _algoInstanceTradeRepository = algoInstanceTradeRepository;
            _algoInstanceTradesHistoryService = algoInstanceTradesHistoryService;
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
    }
}
