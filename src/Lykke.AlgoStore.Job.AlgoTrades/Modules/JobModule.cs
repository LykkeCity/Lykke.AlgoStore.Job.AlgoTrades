using Autofac;
using AzureStorage.Tables;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Entities;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Job.AlgoTrades.RabbitSubscribers;
using Lykke.AlgoStore.Job.AlgoTrades.Settings;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.AlgoStore.Service.AlgoTrades.Services;
using Lykke.AlgoStore.Service.Statistics.Client;
using Lykke.Common.Log;
using Lykke.Sdk.Health;
using Lykke.SettingsReader;

namespace Lykke.AlgoStore.Job.AlgoTrades.Modules
{
    public class JobModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public JobModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterStatisticsClient(_settings.CurrentValue.AlgoStoreStatisticsClient.ServiceUrl);

            RegisterRepositories(builder);
            RegisterRabbitMqSubscribers(builder);
            RegisterRedisPublisher(builder);
            RegisterApplicationServices(builder);
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            builder.Register(x =>
                {
                    var log = x.Resolve<ILogFactory>();
                    var repository = CreateAlgoTradeRepository(
                        _settings.Nested(y => y.AlgoTradesJob.Db.LogsConnString), log);

                    return repository;
                })
                .As<IAlgoInstanceTradeRepository>()
                .SingleInstance();

            builder.Register(x =>
                {
                    var log = x.Resolve<ILogFactory>();
                    var repository = CreateAlgoInstanceRepository(
                        _settings.Nested(y => y.AlgoTradesJob.Db.LogsConnString), log);

                    return repository;
                })
                .As<IAlgoClientInstanceRepository>()
                .SingleInstance();
        }

        private void RegisterRabbitMqSubscribers(ContainerBuilder builder)
        {
            builder.RegisterType<AlgoInstanceTradesSubscriber>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.AlgoTradesJob.Rabbit));

            builder.RegisterType<MatchingEngineOrderEventsSubscriber>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.AlgoTradesJob.MatchingEngineRabbitMq));
        }

        private void RegisterRedisPublisher(ContainerBuilder builder)
        {
            builder.RegisterType<RedisOrderUpdatePublisher>()
                .WithParameter("endPoint", _settings.CurrentValue.AlgoTradesJob.Redis.EndPoint)
                .WithParameter("password", _settings.CurrentValue.AlgoTradesJob.Redis.Password)
                .SingleInstance()
                .As<IOrderUpdatePublisher>();
        }

        private void RegisterApplicationServices(ContainerBuilder builder)
        {
            builder.RegisterType<AlgoInstanceTradesHistoryWriter>()
                .As<IAlgoInstanceTradesHistoryWriter>()
                .SingleInstance();

            builder.RegisterType<AlgoInstanceTradesCountUpdater>()
                .As<IAlgoInstanceTradesCountUpdater>()
                .SingleInstance();
        }

        private static AlgoInstanceTradeRepository CreateAlgoTradeRepository(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            return new AlgoInstanceTradeRepository(
                AzureTableStorage<AlgoInstanceTradeEntity>.Create(connectionString,
                    AlgoInstanceTradeRepository.TableName, logFactory));
        }

        private static AlgoClientInstanceRepository CreateAlgoInstanceRepository(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            return new AlgoClientInstanceRepository(
                AzureTableStorage<AlgoClientInstanceEntity>.Create(
                    connectionString, AlgoClientInstanceRepository.TableName, logFactory),
                AzureTableStorage<AlgoInstanceStoppingEntity>.Create(
                    connectionString, AlgoClientInstanceRepository.TableName, logFactory),
                AzureTableStorage<AlgoInstanceTcBuildEntity>.Create(
                    connectionString, AlgoClientInstanceRepository.TableName, logFactory)
                );
        }
    }
}
