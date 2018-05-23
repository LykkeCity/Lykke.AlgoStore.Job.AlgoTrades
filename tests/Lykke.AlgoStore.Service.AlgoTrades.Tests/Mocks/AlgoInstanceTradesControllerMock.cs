using Lykke.AlgoStore.Service.AlgoTrades.Controllers;

namespace Lykke.AlgoStore.Service.AlgoTrades.Tests.Mocks
{
    public static class AlgoInstanceTradesControllerMock
    {
        public static AlgoInstanceTradesController GetControllerInstance()
        {
            AlgoInstanceTradesController controller = new AlgoInstanceTradesController(
                                                AlgoInstanceTradesRepositoryMock.GetAlgoInstanceTradeRepositoryRepository(),
                                                AlgoInstanceTradesHistoryServiceMock.GetAlgoInstanceTradesHistoryService());
            return controller;
        }
    }
}
