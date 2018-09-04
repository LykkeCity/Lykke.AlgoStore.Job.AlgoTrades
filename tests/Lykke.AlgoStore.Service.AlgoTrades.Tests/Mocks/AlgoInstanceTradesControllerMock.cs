using Common.Log;
using Lykke.AlgoStore.Service.AlgoTrades.Controllers;
using Lykke.Common.Log;
using Moq;

namespace Lykke.AlgoStore.Service.AlgoTrades.Tests.Mocks
{
    public static class AlgoInstanceTradesControllerMock
    {
        public static AlgoInstanceTradesController GetControllerInstance()
        {
            var logFactoryMock = new Mock<ILogFactory>();
            var controller = new AlgoInstanceTradesController(
                AlgoInstanceTradesRepositoryMock.GetAlgoInstanceTradeRepositoryRepository(),
                AlgoInstanceTradesHistoryServiceMock.GetAlgoInstanceTradesHistoryService(), logFactoryMock.Object);

            return controller;
        }
    }
}
