using Autofac;
using Common;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Job.AlgoTrades.Settings.JobSettings;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.OperationsRepository.Contract;
using Lykke.Service.OperationsRepository.Contract.Cash;
using Lykke.Service.OperationsRepository.Contract.History;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Lykke.Common.Log;

namespace Lykke.AlgoStore.Job.AlgoTrades.RabbitSubscribers
{
    /// <summary>
    /// Algo Instance subscriber which will proceed information about trades.
    /// We get information about every Lykke trade. 
    /// </summary>
    public class AlgoInstanceTradesSubscriber : IStartable, IStopable
    {
        private readonly IAlgoInstanceTradeRepository _instanceTradeRepository;
        private readonly IAlgoInstanceTradesHistoryWriter _algoTradesHistoryWriter;
        private readonly IAlgoInstanceTradesCountUpdater _tradesCountUpdater;
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _rabbitSettings;

        private RabbitMqSubscriber<OperationsHistoryMessage> _subscriber;

        public AlgoInstanceTradesSubscriber(
            IAlgoInstanceTradeRepository instanceTradeRepository,
            IAlgoInstanceTradesHistoryWriter algoTradesHistoryWriter,
            IAlgoInstanceTradesCountUpdater tradesCountUpdater,
            ILogFactory logFactory,
            RabbitMqSettings rabbitSettings)
        {
            _instanceTradeRepository = instanceTradeRepository;
            _algoTradesHistoryWriter = algoTradesHistoryWriter;
            _tradesCountUpdater = tradesCountUpdater;
            _logFactory = logFactory;
            _rabbitSettings = rabbitSettings;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings.CreateForSubscriber(_rabbitSettings.ConnectionString,
                                    _rabbitSettings.ExchangeOperationsHistory, 
                                    _rabbitSettings.QueueAlgoTradesUpdater);
            settings.MakeDurable();

            _subscriber = new RabbitMqSubscriber<OperationsHistoryMessage>(
                    _logFactory,
                    settings,
                    new ResilientErrorHandlingStrategy(
                        _logFactory, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<OperationsHistoryMessage>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        private async Task ProcessMessageAsync(OperationsHistoryMessage message)
        {
            var operationType = (OperationType)Enum.Parse(typeof(OperationType), message.OpType);

            if (operationType != OperationType.ClientTrade) return;

            var clientTrade = JsonConvert.DeserializeObject<ClientTradeDto>(message.Data);

            if (string.IsNullOrEmpty(clientTrade.MarketOrderId)) return;

            var algoInstanceOrder = await _instanceTradeRepository
                    .GetAlgoInstanceOrderAsync(clientTrade.MarketOrderId, clientTrade.ClientId);

            if (algoInstanceOrder == null) return;

            var log = _logFactory.CreateLog(this);

            log.Info("Trade save started");

            await _algoTradesHistoryWriter.SaveAsync(clientTrade, algoInstanceOrder);

            log.Info("Trade save finished");
            log.Info("TotalNumberOfTrades update started");

            await _tradesCountUpdater.IncreaseInstanceTradeCountAsync(clientTrade, algoInstanceOrder);

            log.Info("TotalNumberOfTrades update finished");
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }
    }
}
