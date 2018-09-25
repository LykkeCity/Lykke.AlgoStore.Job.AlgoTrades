using Autofac;
using Common;
using Common.Log;
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
        private readonly ILog _log;
        private readonly RabbitMqSettings _rabbitSettings;

        private RabbitMqSubscriber<OperationsHistoryMessage> _subscriber;

        public AlgoInstanceTradesSubscriber(
            IAlgoInstanceTradeRepository instanceTradeRepository,
            IAlgoInstanceTradesHistoryWriter algoTradesHistoryWriter,
            IAlgoInstanceTradesCountUpdater tradesCountUpdater,
            ILog log,
            RabbitMqSettings rabbitSettings)
        {
            _instanceTradeRepository = instanceTradeRepository;
            _algoTradesHistoryWriter = algoTradesHistoryWriter;
            _tradesCountUpdater = tradesCountUpdater;
            _log = log;
            _rabbitSettings = rabbitSettings;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings.CreateForSubscriber(_rabbitSettings.ConnectionString,
                                    _rabbitSettings.ExchangeOperationsHistory, 
                                    _rabbitSettings.QueueAlgoTradesUpdater);
            settings.MakeDurable();

            _subscriber = new RabbitMqSubscriber<OperationsHistoryMessage>(settings,
                    new ResilientErrorHandlingStrategy(_log, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_log, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<OperationsHistoryMessage>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .SetLogger(_log)
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

            _log.Info("Trade save started");

            await _algoTradesHistoryWriter.SaveAsync(clientTrade, algoInstanceOrder);

            _log.Info("Trade save finished");
            _log.Info("TotalNumberOfTrades update started");

            await _tradesCountUpdater.IncreaseInstanceTradeCountAsync(clientTrade, algoInstanceOrder);

            _log.Info("TotalNumberOfTrades update finished");
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
