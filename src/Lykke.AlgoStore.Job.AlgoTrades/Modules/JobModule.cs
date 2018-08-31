using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Entities;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Job.AlgoTrades.RabbitSubscribers;
using Lykke.AlgoStore.Job.AlgoTrades.Settings;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.AlgoStore.Service.AlgoTrades.Services;
using Lykke.Common.Log;
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
            RegisterRepositories(builder);
            RegisterRabbitMqSubscribers(builder);
            RegisterApplicationServices(builder);
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            //builder.RegisterInstance<IAlgoInstanceTradeRepository>(CreateAlgoTradeRepository(
            //    _settings.Nested(x => x.AlgoTradesJob.Db.LogsConnString), _log)).SingleInstance();

            builder.Register(x =>
                {
                    var log = x.Resolve<ILogFactory>();
                    var repository = CreateAlgoTradeRepository(
                        _settings.Nested(y => y.AlgoTradesJob.Db.LogsConnString), log);

                    return repository;
                })
                .As<IAlgoInstanceTradeRepository>()
                .SingleInstance();
        }

        private void RegisterRabbitMqSubscribers(ContainerBuilder builder)
        {
            //builder.RegisterType<AlgoInstanceTradesSubscriber>()
            //    .As<IStartable>()
            //    .AutoActivate()
            //    .SingleInstance()
            //    .WithParameter(TypedParameter.From(_settings.CurrentValue.AlgoTradesJob.Rabbit));

            builder.RegisterType<MatchingEngineOrderEventsSubscriber>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }

        private void RegisterApplicationServices(ContainerBuilder builder)
        {
            builder.RegisterType<AlgoInstanceTradesHistoryWriter>()
                .As<IAlgoInstanceTradesHistoryWriter>()
                .SingleInstance();
        }

        private static AlgoInstanceTradeRepository CreateAlgoTradeRepository(IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            return new AlgoInstanceTradeRepository(
                AzureTableStorage<AlgoInstanceTradeEntity>.Create(connectionString,
                    AlgoInstanceTradeRepository.TableName, logFactory));
        }
    }
}
