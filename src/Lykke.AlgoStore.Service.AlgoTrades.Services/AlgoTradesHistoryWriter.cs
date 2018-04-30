using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.Service.OperationsRepository.Contract;
using Lykke.Service.OperationsRepository.Contract.Cash;
using Lykke.Service.OperationsRepository.Contract.History;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Service.AlgoTrades.Services
{
    public class AlgoTradesHistoryWriter : IAlgoTradesHistoryWriter
    {
        private readonly IAlgoInstanceTradeRepository _algoInstanceTradeRepository;

        public AlgoTradesHistoryWriter(IAlgoInstanceTradeRepository algoInstanceTradeRepository)
        {
            _algoInstanceTradeRepository = algoInstanceTradeRepository;
        }

        public async Task SaveAsync(OperationsHistoryMessage historyRecord)
        {
            //Save trade history to algo db table...when IAlgoInstanceTradeRepository is ready

            var operationType = (OperationType)Enum.Parse(typeof(OperationType), historyRecord.OpType);

            if (operationType == OperationType.ClientTrade)
            {
                var clientTrade = JsonConvert.DeserializeObject<ClientTradeDto>(historyRecord.Data);

                AlgoInstanceTrade algoInstanceOrder = null;

                if (!string.IsNullOrEmpty(clientTrade.MarketOrderId))
                    algoInstanceOrder = await _algoInstanceTradeRepository.GetAlgoInstanceOrderAsync(clientTrade.MarketOrderId, clientTrade.ClientId);

                if (algoInstanceOrder != null)
                {
                    AlgoInstanceTrade trade = new AlgoInstanceTrade()
                    {
                        InstanceId = algoInstanceOrder.InstanceId,
                        AssetId = clientTrade.AssetId,
                        AssetPairId = clientTrade.AssetPairId,
                        Fee = clientTrade.FeeSize,
                        Amount = clientTrade.Amount,
                        WalletId = algoInstanceOrder.WalletId,
                        IsBuy = algoInstanceOrder.IsBuy,
                        Price = algoInstanceOrder.Price
                    };

                    await _algoInstanceTradeRepository.SaveAlgoInstanceTradeAsync(trade);
                }
            }
        }
    }

}
