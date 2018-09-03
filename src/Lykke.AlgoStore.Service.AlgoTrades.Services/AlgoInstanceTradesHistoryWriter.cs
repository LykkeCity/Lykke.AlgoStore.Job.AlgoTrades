using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.Service.OperationsRepository.Contract;
using Lykke.Service.OperationsRepository.Contract.Cash;
using Lykke.Service.OperationsRepository.Contract.History;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Models.Events;
using OrderType = Lykke.MatchingEngine.Connector.Models.Events.OrderType;

namespace Lykke.AlgoStore.Service.AlgoTrades.Services
{
    public class AlgoInstanceTradesHistoryWriter : IAlgoInstanceTradesHistoryWriter
    {
        private readonly IAlgoInstanceTradeRepository _algoInstanceTradeRepository;

        public AlgoInstanceTradesHistoryWriter(IAlgoInstanceTradeRepository algoInstanceTradeRepository)
        {
            _algoInstanceTradeRepository = algoInstanceTradeRepository;
        }

        /// <summary>
        /// Save algo instance trade details when there is a new trade for existing order/row in AlgoStore database.
        /// </summary>
        /// <param name="historyRecord">Historical data which comes from RabbitMQ exchange</param>
        public async Task SaveAsync(OperationsHistoryMessage historyRecord)
        {
            var operationType = (OperationType) Enum.Parse(typeof(OperationType), historyRecord.OpType);

            if (operationType == OperationType.ClientTrade)
            {
                var clientTrade = JsonConvert.DeserializeObject<ClientTradeDto>(historyRecord.Data);

                AlgoInstanceTrade algoInstanceOrder = null;

                if (!string.IsNullOrEmpty(clientTrade.MarketOrderId))
                    algoInstanceOrder =
                        await _algoInstanceTradeRepository.GetAlgoInstanceOrderAsync(clientTrade.MarketOrderId,
                            clientTrade.ClientId);

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
                        OrderId = algoInstanceOrder.OrderId,
                        IsBuy = algoInstanceOrder.IsBuy,
                        Price = algoInstanceOrder.Price,
                        DateOfTrade = clientTrade.DateTime
                    };

                    await _algoInstanceTradeRepository.SaveAlgoInstanceTradeAsync(trade);
                }
            }
        }

        public async Task SaveAsync(Order order)
        {
            //Check if this is a limit order
            //REMARK: We will deal just with LIMIT orders in this method (for now)
            //In future, we have a task for this, we will move market limit orders login in here too
            //(we will stop to use orders history for market orders and make them use new ME)
            if (order.OrderType != OrderType.Limit)
                return;

            var algoInstanceOrder =
                await _algoInstanceTradeRepository.GetAlgoInstanceOrderAsync(order.Id, order.WalletId);

            //If this is not AlgoStore order just ignore it
            if (algoInstanceOrder == null)
                return;

            //If order does not have any trades then ignore it :)
            if (!order.Trades.Any())
                return;

            foreach (var orderTrade in order.Trades)
            {
                //Map ME order to AlgoInstanceTrade
                var trade = new AlgoInstanceTrade
                {
                    InstanceId = algoInstanceOrder.InstanceId,
                    AssetId = orderTrade.BaseAssetId,
                    AssetPairId = order.AssetPairId,
                    Fee = orderTrade.Fees.Any()
                        ? orderTrade.Fees.Where(x => !string.IsNullOrEmpty(x.Volume)).Sum(x => double.Parse(x.Volume))
                        : (double?) null,
                    Amount = string.IsNullOrEmpty(orderTrade.BaseVolume) ? (double?)null : double.Parse(orderTrade.BaseVolume),
                    WalletId = algoInstanceOrder.WalletId,
                    OrderId = algoInstanceOrder.OrderId,
                    IsBuy = algoInstanceOrder.IsBuy,
                    Price = algoInstanceOrder.Price,
                    DateOfTrade = orderTrade.Timestamp
                };

                await _algoInstanceTradeRepository.SaveAlgoInstanceTradeAsync(trade);
            }
        }
    }
}
