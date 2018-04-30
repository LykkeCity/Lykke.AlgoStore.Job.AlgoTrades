using System.Net;
using Lykke.Common.Api.Contract.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.AlgoStore.Service.AlgoTrades.Controllers
{
    [Route("api/algoInstanceTrades")]
    public class AlgoInstanceTradesController : Controller
    {
        /// <summary>
        ///Returns Algo Instance trades
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("GetAlgoInstanceTrades")]
        [ProducesResponseType(typeof(IsAliveResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult Get([FromQuery] string instanceId, [FromQuery]string assetId)
        {

            return Ok();
        }
    }
}
