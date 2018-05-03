using Lykke.AlgoStore.Service.AlgoTrades.Controllers;
using Lykke.AlgoStore.Service.AlgoTrades.Tests.Mocks;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;

namespace Lykke.AlgoStore.Service.AlgoTrades.Tests
{
    public class GetAlgoInstanceTradesControllerTests
    {
        private readonly string _mockInstanceId = "1xd49482-2108-4b39-97ed-61ca1f4df410";
        private readonly string _tradedAssetId = "USD";

        [Fact]
        public async Task GetTrades_ResponseOk()
        {
            AlgoInstanceTradesController controller = AlgoInstanceTradesControllerMock.GetControllerInstance();

            var result = await controller.Get(_mockInstanceId, _tradedAssetId, 10);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetTrades_BadResponse()
        {
            AlgoInstanceTradesController controller = AlgoInstanceTradesControllerMock.GetControllerInstance();

            var result = await controller.Get(_mockInstanceId, _tradedAssetId, 0);
            Assert.IsType<BadRequestObjectResult>(result);

            var resultBigNumber = await controller.Get(_mockInstanceId, _tradedAssetId, 5000);
            Assert.IsType<BadRequestObjectResult>(resultBigNumber);
        }
    }
}
