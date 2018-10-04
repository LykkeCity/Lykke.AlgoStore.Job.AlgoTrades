using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.Common.Log;
using MongoDB.Bson;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace Lykke.AlgoStore.Service.AlgoTrades.Services
{
    public class RedisOrderUpdatePublisher : IOrderUpdatePublisher
    {
        private readonly ISubscriber _redisPublisher;
        private readonly ILog _log;
        private readonly NewtonsoftSerializer _messageSerializer;

        public RedisOrderUpdatePublisher(string endPoint, string password, ILogFactory logFactory)
        {
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { endPoint },
                Password = password,
                Ssl = false,
                AbortOnConnectFail = false,
                AllowAdmin = false,
                ReconnectRetryPolicy = new LinearRetry(5000),
            };

            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(configurationOptions);
            _redisPublisher = connection.GetSubscriber();

            _log = logFactory.CreateLog(this);

            _messageSerializer = new NewtonsoftSerializer();
        }

        public async Task Publish(AlgoInstanceTrade orderTrade)
        {
            try
            {
                var orderUpdate = await _messageSerializer.SerializeAsync(orderTrade);

                var receivers =  await _redisPublisher.PublishAsync(new RedisChannel(orderTrade.InstanceId, RedisChannel.PatternMode.Literal), orderUpdate, CommandFlags.DemandMaster);

                _log.Info($"Order update received for order Id {orderTrade.Id}, instanceId {orderTrade.InstanceId}. Number of receivers the event was sent to: {receivers} ", orderTrade.ToJson());
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unable to publish order update message to Redis", orderTrade.ToJson());
            }
        }
    }
}
