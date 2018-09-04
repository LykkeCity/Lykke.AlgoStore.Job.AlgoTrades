using Autofac;
using Common;
using Lykke.AlgoStore.Job.AlgoTrades.Settings.JobSettings;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.OperationsRepository.Contract.History;
using System;
using System.Threading.Tasks;
using Lykke.Common.Log;

namespace Lykke.AlgoStore.Job.AlgoTrades.RabbitSubscribers
{
    /// <summary>
    /// Algo Instance subscriber which will proceed information about trades. We get information about every Lykke trade. 
    /// </summary>
    public class AlgoInstanceTradesSubscriber : IStartable, IStopable
    {
        private readonly IAlgoInstanceTradesHistoryWriter _algoTradesHistoryWriter;
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _rabbitSettings;

        private RabbitMqSubscriber<OperationsHistoryMessage> _subscriber;

        public AlgoInstanceTradesSubscriber(
            IAlgoInstanceTradesHistoryWriter algoTradesHistoryWriter,
            ILogFactory logFactory,
            RabbitMqSettings rabbitSettings)
        {
            _algoTradesHistoryWriter = algoTradesHistoryWriter;
            _logFactory = logFactory;
            _rabbitSettings = rabbitSettings;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings.CreateForSubscriber(_rabbitSettings.ConnectionString,
                                    _rabbitSettings.ExchangeOperationsHistory, _rabbitSettings.QueueAlgoTradesUpdater);
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
