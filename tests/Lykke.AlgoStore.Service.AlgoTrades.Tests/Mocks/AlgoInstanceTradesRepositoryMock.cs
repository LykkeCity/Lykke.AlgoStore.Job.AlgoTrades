using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;

namespace Lykke.AlgoStore.Service.AlgoTrades.Tests.Mocks
{
    public static class AlgoInstanceTradesRepositoryMock
    {
        public static string MockedWalletId => _mockWalletId;
        public static string MockedInstanceId => _mockInstanceId;
        public static string MockedOrderId => _mockOrderId;


        private static readonly string _mockWalletId = "1ed49482-2108-4b39-97ed-61ca1f4df510";
        private static readonly string _mockInstanceId = "1xd49482-2108-4b39-97ed-61ca1f4df410";
        private static readonly string _mockOrderId = "32d49482-21sz-4b39-14ed-61ca1f4df510";

        public static IAlgoInstanceTradeRepository GetAlgoClientInstanceRepository()
        {
            var repoMock = new Mock<IAlgoInstanceTradeRepository>();

            repoMock.Setup(r => r.GetAlgoInstaceTradesByTradedAssetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(GetByTradedAsset()));

            return repoMock.Object;
        }

        private static IEnumerable<AlgoInstanceTrade> GetByTradedAsset()
        {
            var result = new List<AlgoInstanceTrade>();
            result.Add(new AlgoInstanceTrade()
            {
                Amount = 5,
                AssetId = "USD",
                AssetPairId = "BTCUSD",
                Fee = 0.05,
                IsBuy = true,
                Price = 6500,
                WalletId = _mockWalletId,
                InstanceId = _mockInstanceId,
                OrderId = _mockOrderId
            });

            return result;
        }
    }
}
