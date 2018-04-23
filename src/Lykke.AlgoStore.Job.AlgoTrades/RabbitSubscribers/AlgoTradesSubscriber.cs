using Autofac;
using Common;
using Common.Log;
using Lykke.AlgoStore.Job.AlgoTrades.Settings.JobSettings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.OperationsRepository.Contract.History;
using System;
using System.Threading.Tasks;
using Lykke.AlgoStore.Job.AlgoTrades.Core.Services;

namespace Lykke.AlgoStore.Job.AlgoTrades.RabbitSubscribers
{
    public class AlgoTradesSubscriber : IStartable, IStopable
    {
        private readonly IAlgoTradesHistoryWriter _algoTradesHistoryWriter;
        private readonly ILog _log;
        private readonly RabbitMqSettings _rabbitSettings;

        private RabbitMqSubscriber<OperationsHistoryMessage> _subscriber;

        public AlgoTradesSubscriber(
            IAlgoTradesHistoryWriter algoTradesHistoryWriter,
            ILog log,
            RabbitMqSettings rabbitSettings)
        {
            _algoTradesHistoryWriter = algoTradesHistoryWriter;
            _log = log;
            _rabbitSettings = rabbitSettings;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings.CreateForSubscriber(_rabbitSettings.ConnectionString,
                                    _rabbitSettings.ExchangeOperationsHistory, _rabbitSettings.QueueAlgoTradesUpdater);
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
            await _algoTradesHistoryWriter.SaveAsync(message);
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
