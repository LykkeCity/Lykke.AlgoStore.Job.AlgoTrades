using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Domain;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Moq;

namespace Lykke.AlgoStore.Service.AlgoTrades.Tests.Mocks
{
    public static class AlgoInstanceTradesHistoryServiceMock
    {
        public static IAlgoInstanceTradesHistoryService GetAlgoClientInstanceRepository()
        {
            var repoMock = new Mock<IAlgoInstanceTradesHistoryService>();

            repoMock.Setup(s => s.ExecuteAsync(It.IsAny<AlgoInstanceTrade>()))
                .Returns(Task.FromResult(GetByTradedAsset()));

            return repoMock.Object;
        }

        private static AlgoInstanceTradeResponseModel GetByTradedAsset()
        {
            return new AlgoInstanceTradeResponseModel()
            {
                Amount = 5,
                TradedAssetName = "USD",
                AssetPair = "BTC/USD",
                Fee = 0.05,
                IsBuy = true,
                Price = 6500,
                WalletId = AlgoInstanceTradesRepositoryMock.MockedWalletId,
                InstanceId = AlgoInstanceTradesRepositoryMock.MockedInstanceId,
                OrderId = AlgoInstanceTradesRepositoryMock.MockedOrderId
            };
        }
    }
}
