using Lykke.AlgoStore.Service.AlgoTrades.Services;
using Lykke.AlgoStore.Service.AlgoTrades.Tests.Mocks;
using Lykke.Service.OperationsRepository.Contract.Cash;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Lykke.AlgoStore.Service.AlgoTrades.Tests
{
    public class AlgoInstanceTradesHistoryWriterTest
    {
        private static readonly ClientTradeDto _data = new ClientTradeDto
        {
            Id = "201805031505_a5982bfa-0f6f-4aef-984b-8634bf40a1m6",
            ClientId = AlgoInstanceTradesRepositoryMock.MockedWalletId,
            DateTime = DateTime.Parse(AlgoInstanceTradesRepositoryMock.MockedDateString),
            IsHidden = false,
            LimitOrderId = null,
            MarketOrderId = AlgoInstanceTradesRepositoryMock.MockedOrderId,
            Amount = AlgoInstanceTradesRepositoryMock.MockedAmount,
            AssetId = "USD",
            AssetPairId = "BTCUSD",
            BlockChainHash = null,
            Multisig = null,
            TransactionId = "1ae4ec41-34f0-4773-be75-04215df3d55e",
            AddressFrom = null,
            AddressTo = null,
            IsSettled = null,
            State = Lykke.Service.OperationsRepository.Contract.TransactionStates.SettledOffchain,
            Price = AlgoInstanceTradesRepositoryMock.MockedPrice,
            DetectionTime = null,
            Confirmations = 0,
            OppositeLimitOrderId = null,
            IsLimitOrderResult = true,
            FeeSize = AlgoInstanceTradesRepositoryMock.MockedFee,
            FeeType = Lykke.Service.OperationsRepository.Contract.Abstractions.FeeType.Unknown
        };

        [Fact]
        public async Task HistoryWiter_Save()
        {
            var message = _data;
            var algoInstanceTradeRepo = AlgoInstanceTradesRepositoryMock.GetAlgoInstanceTradeRepositoryRepository_ForHistoryWriter();
            var order = await algoInstanceTradeRepo.GetAlgoInstanceOrderAsync(message.MarketOrderId, message.ClientId);

            var writer = new AlgoInstanceTradesHistoryWriter(algoInstanceTradeRepo);

            await writer.SaveAsync(message, order);
        }
    }
}
