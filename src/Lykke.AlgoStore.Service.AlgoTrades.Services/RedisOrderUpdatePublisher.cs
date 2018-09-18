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

namespace Lykke.AlgoStore.Service.AlgoTrades.Services
{
    public class RedisOrderUpdatePublisher : IOrderUpdatePublisher
    {
        private readonly ISubscriber _redisPublisher;
        private readonly ILog _log;

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
        }

        public async Task Publish(AlgoInstanceTrade orderTrade)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, orderTrade);
                    await _redisPublisher.PublishAsync(new RedisChannel(orderTrade.InstanceId, RedisChannel.PatternMode.Literal), RedisValue.CreateFrom(stream), CommandFlags.DemandMaster);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Unable to publish order update message to Redis", orderTrade);
            }
        }
    }
}
