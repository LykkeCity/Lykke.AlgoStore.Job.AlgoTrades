using Common.Log;
using Lykke.AlgoStore.Service.AlgoTrades.Services;
using Lykke.AlgoStore.Service.AlgoTrades.Tests.Mocks;
using Lykke.Service.OperationsRepository.Contract.History;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Lykke.AlgoStore.Service.AlgoTrades.Tests
{
    public class AlgoInstanceTradesHistoryWriterTest
    {
        private static readonly string _data = "{\"Id\":\"201805031505_a5982bfa-0f6f-4aef-984b-8634bf40a1m6\"," +
                                               "\"ClientId\":\"" + AlgoInstanceTradesRepositoryMock.MockedWalletId + "\"," +
                                               "\"DateTime\":\"2018-05-03T15:05:12.225Z\",\"IsHidden\":false," +
                                               "\"LimitOrderId\":\"null\",\"MarketOrderId\":\"" + AlgoInstanceTradesRepositoryMock.MockedOrderId + "\"," +
                                               "\"Amount\":\"" + AlgoInstanceTradesRepositoryMock.MockedAmount + "\",\"AssetId\":\"USD\",\"AssetPairId\":\"BTCUSD\"," +
                                               "\"BlockChainHash\":null,\"Multisig\":null,\"TransactionId\":\"1ae4ec41-34f0-4773-be75-04215df3d55e\"," +
                                               "\"AddressFrom\":null,\"AddressTo\":null,\"IsSettled\":null,\"State\":3,\"Price\":\""
                                               + AlgoInstanceTradesRepositoryMock.MockedPrice + "\",\"DetectionTime\":null,\"Confirmations\":0,\"OppositeLimitOrderId\":\"null\",\"IsLimitOrderResult\":true," +
                                               "\"FeeSize\":\"" + AlgoInstanceTradesRepositoryMock.MockedFee + "\",\"FeeType\":0}";

        private static OperationsHistoryMessage GetMessageMock()
        {
            return new OperationsHistoryMessage()
            {
                OpType = "ClientTrade",
                DateTime = DateTime.Now.Date,
                ClientId = AlgoInstanceTradesRepositoryMock.MockedWalletId,
                Amount = 5,
                Data = _data
            };
        }

        [Fact]
        public async Task HistoryWiter_Save()
        {
            var message = GetMessageMock();
            var algoInstanceTradeRepo = AlgoInstanceTradesRepositoryMock.GetAlgoInstanceTradeRepositoryRepository_ForHistoryWriter();

            AlgoInstanceTradesHistoryWriter writer = new AlgoInstanceTradesHistoryWriter(algoInstanceTradeRepo);

            await writer.SaveAsync(message);
        }
    }
}
