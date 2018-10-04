using System;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.Service.OperationsRepository.Contract.Cash;
using System.Linq;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Models.Events;
using OrderType = Lykke.MatchingEngine.Connector.Models.Events.OrderType;

namespace Lykke.AlgoStore.Service.AlgoTrades.Services
{
    public class AlgoInstanceTradesHistoryWriter : IAlgoInstanceTradesHistoryWriter
    {
        private readonly IAlgoInstanceTradeRepository _algoInstanceTradeRepository;
        private readonly IOrderUpdatePublisher _orderUpdatePublisher;

        public AlgoInstanceTradesHistoryWriter(IAlgoInstanceTradeRepository algoInstanceTradeRepository, IOrderUpdatePublisher orderUpdatePublisher)
        {
            _algoInstanceTradeRepository = algoInstanceTradeRepository;
            _orderUpdatePublisher = orderUpdatePublisher;
        }

        /// <summary>
        /// Save algo instance trade details when there is a new trade for existing order/row in AlgoStore database.
        /// </summary>
        /// <param name="instanceTrade">Historical data which comes from RabbitMQ exchange</param>
        public async Task SaveAsync(ClientTradeDto clientTrade, AlgoInstanceTrade algoInstanceOrder)
        {
            var trade = new AlgoInstanceTrade()
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

            algoInstanceOrder.OrderStatus = Enum.TryParse<CSharp.AlgoTemplate.Models.Enumerators.OrderStatus>(order.Status.ToString(), out var status) ? status : CSharp.AlgoTemplate.Models.Enumerators.OrderStatus.UnknownStatus;
            await _algoInstanceTradeRepository.CreateOrUpdateAlgoInstanceOrderAsync(algoInstanceOrder);

            //If order does not have any trades then ignore it :)
            if (order.Trades == null || !order.Trades.Any())
                return;

            foreach (var orderTrade in order.Trades)
            {
                //Map ME order to AlgoInstanceTrade
                var trade = new AlgoInstanceTrade
                {
                    InstanceId = algoInstanceOrder.InstanceId,
                    AssetId = orderTrade.BaseAssetId,
                    AssetPairId = order.AssetPairId,
                    Fee = orderTrade.Fees!=null && orderTrade.Fees.Any()
                        ? orderTrade.Fees.Where(x => !string.IsNullOrEmpty(x.Volume)).Sum(x => double.Parse(x.Volume))
                        : (double?) null,
                    Amount = string.IsNullOrEmpty(orderTrade.BaseVolume)
                        ? (double?) null
                        : double.Parse(orderTrade.BaseVolume),
                    WalletId = algoInstanceOrder.WalletId,
                    OrderId = algoInstanceOrder.OrderId,
                    IsBuy = algoInstanceOrder.IsBuy,
                    Price = algoInstanceOrder.Price,
                    DateOfTrade = orderTrade.Timestamp
                };

                await _algoInstanceTradeRepository.SaveAlgoInstanceTradeAsync(trade);
                await _orderUpdatePublisher.Publish(trade);
            }
        }
    }
}
