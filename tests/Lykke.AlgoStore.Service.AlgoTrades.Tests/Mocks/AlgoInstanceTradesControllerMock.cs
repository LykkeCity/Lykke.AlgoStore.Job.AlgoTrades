using Common.Log;
using Lykke.AlgoStore.Service.AlgoTrades.Controllers;
using Moq;

namespace Lykke.AlgoStore.Service.AlgoTrades.Tests.Mocks
{
    public static class AlgoInstanceTradesControllerMock
    {
        public static AlgoInstanceTradesController GetControllerInstance()
        {
            var repoMock = new Mock<ILog>();
            AlgoInstanceTradesController controller = new AlgoInstanceTradesController(
                                                AlgoInstanceTradesRepositoryMock.GetAlgoInstanceTradeRepositoryRepository(),
                                                AlgoInstanceTradesHistoryServiceMock.GetAlgoInstanceTradesHistoryService(), repoMock.Object);
            return controller;
        }
    }
}
