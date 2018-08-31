using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
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

        public MatchingEngineOrderEventsSubscriber(ILogFactory logFactory,
            IAlgoInstanceTradesHistoryWriter algoTradesHistoryWriter)
        {
            _logFactory = logFactory;
            _algoTradesHistoryWriter = algoTradesHistoryWriter;
        }

        public void Start()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = "amqp://lykke.history:lykke.history@rabbit-me.lykke-me.svc.cluster.local:5672",
                ExchangeName = "lykke.spot.matching.engine.out.events",
                QueueName = "lykke.algo-store.job.trades.Order",
                RoutingKey = "4", //order
                IsDurable = true
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

            //foreach (var order in executionEvent.Orders)
            //{
            //    //REMARK: Check if this will work with both market and limit orders
            //    await _algoTradesHistoryWriter.SaveAsync(order);
            //}

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
