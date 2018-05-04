using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Lykke.AlgoStore.Service.AlgoTrades.Tests.Mocks
{
    public static class AlgoInstanceTradesRepositoryMock
    {
        public static string MockedWalletId => _mockWalletId;
        public static string MockedInstanceId => _mockInstanceId;
        public static string MockedOrderId => _mockOrderId;
        public static double MockedAmount => _mockedAmount;
        public static double MockedFee => _mockedFee;
        public static double MockedPrice => _mockedPrice;

        private static readonly string _mockWalletId = "1ed49482-2108-4b39-97ed-61ca1f4df510";
        private static readonly string _mockInstanceId = "1xd49482-2108-4b39-97ed-61ca1f4df410";
        private static readonly string _mockOrderId = "32d49482-21sz-4b39-14ed-61ca1f4df510";
        private static readonly double _mockedAmount = 5;
        private static readonly double _mockedFee = 0.05;
        private static readonly double _mockedPrice = 5000;

        public static IAlgoInstanceTradeRepository GetAlgoInstanceTradeRepositoryRepository()
        {
            var repoMock = new Mock<IAlgoInstanceTradeRepository>();

            repoMock.Setup(r => r.GetAlgoInstaceTradesByTradedAssetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(GetByTradedAsset()));

            return repoMock.Object;
        }

        public static IAlgoInstanceTradeRepository GetAlgoInstanceTradeRepositoryRepository_ForHistoryWriter()
        {
            var repoMock = new Mock<IAlgoInstanceTradeRepository>();
            var mockedTrade = GetTestTrade();

            repoMock.Setup(r => r.GetAlgoInstanceOrderAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(GetOrder()));

            repoMock.Setup(r => r.SaveAlgoInstanceTradeAsync(It.IsAny<AlgoInstanceTrade>())).Returns((AlgoInstanceTrade trade) =>
            {
                CheckIsEqualClientId(trade, mockedTrade);
                return Task.CompletedTask;
            });

            return repoMock.Object;
        }

        private static void CheckIsEqualClientId(AlgoInstanceTrade savedTrade, AlgoInstanceTrade testTrade)
        {
            string first = JsonConvert.SerializeObject(savedTrade);
            string second = JsonConvert.SerializeObject(testTrade);

            Assert.Equal(first, second);
        }

        private static IEnumerable<AlgoInstanceTrade> GetByTradedAsset()
        {
            var result = new List<AlgoInstanceTrade>();
            result.Add(new AlgoInstanceTrade()
            {
                Amount = _mockedAmount,
                AssetId = "USD",
                AssetPairId = "BTCUSD",
                Fee = _mockedFee,
                IsBuy = true,
                Price = _mockedPrice,
                WalletId = _mockWalletId,
                InstanceId = _mockInstanceId,
                OrderId = _mockOrderId
            });

            return result;
        }

        private static AlgoInstanceTrade GetOrder()
        {
            return new AlgoInstanceTrade()
            {
                AssetId = "USD",
                AssetPairId = "BTCUSD",
                IsBuy = true,
                WalletId = _mockWalletId,
                InstanceId = _mockInstanceId,
                OrderId = _mockOrderId,
                Price = _mockedPrice,
                Amount = 5,
            };
        }

        private static AlgoInstanceTrade GetTestTrade()
        {
            return new AlgoInstanceTrade()
            {
                Amount = _mockedAmount,
                AssetId = "USD",
                AssetPairId = "BTCUSD",
                Fee = 0.05,
                IsBuy = true,
                Price = _mockedPrice,
                WalletId = _mockWalletId,
                InstanceId = _mockInstanceId,
                OrderId = _mockOrderId
            };
        }
    }
}
