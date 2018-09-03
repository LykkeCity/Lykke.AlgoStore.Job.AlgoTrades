using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.AlgoStore.Job.AlgoTrades.Settings.JobSettings;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Models.Events;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.AlgoStore.Job.AlgoTrades.RabbitSubscribers
{
    public class MatchingEngineOrderEventsSubscriber : IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private RabbitMqSubscriber<ExecutionEvent> _subscriber;
        private readonly IAlgoInstanceTradesHistoryWriter _algoTradesHistoryWriter;
        private readonly MatchingEngineRabbitMqSettings _rabbitSettings;

        public MatchingEngineOrderEventsSubscriber(ILogFactory logFactory,
            IAlgoInstanceTradesHistoryWriter algoTradesHistoryWriter,
            MatchingEngineRabbitMqSettings rabbitSettings)
        {
            _logFactory = logFactory;
            _algoTradesHistoryWriter = algoTradesHistoryWriter;
            _rabbitSettings = rabbitSettings;
        }

        public void Start()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _rabbitSettings.ConnectionString,
                ExchangeName = _rabbitSettings.ExchangeName,
                QueueName = _rabbitSettings.QueueName,
                RoutingKey = _rabbitSettings.RoutingKey, //4 - order
                IsDurable = _rabbitSettings.IsDurable
            };

            _subscriber = new RabbitMqSubscriber<ExecutionEvent>(
                    _logFactory,
                    settings,
                    new ResilientErrorHandlingStrategy(
                        _logFactory, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new ProtobufMessageDeserializer<ExecutionEvent>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        private async Task ProcessMessageAsync(ExecutionEvent executionEvent)
        {
            if (!executionEvent.Orders.Any())
                return;

            var tasks = executionEvent.Orders.Select(x => _algoTradesHistoryWriter.SaveAsync(x));

            await Task.WhenAll(tasks);
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
