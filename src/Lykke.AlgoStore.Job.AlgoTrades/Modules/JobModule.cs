using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Entities;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Job.AlgoTrades.RabbitSubscribers;
using Lykke.AlgoStore.Job.AlgoTrades.Settings;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.AlgoStore.Service.AlgoTrades.Services;
using Lykke.SettingsReader;

namespace Lykke.AlgoStore.Job.AlgoTrades.Modules
{
    public class JobModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public JobModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            RegisterRepositories(builder);
            RegisterRabbitMqSubscribers(builder);
            RegisterApplicationServices(builder);
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            builder.RegisterInstance<IAlgoInstanceTradeRepository>(CreateAlgoTradeRepository(
                _settings.Nested(x => x.AlgoTradesJob.Db.LogsConnString), _log)).SingleInstance();

        }

        private void RegisterRabbitMqSubscribers(ContainerBuilder builder)
        {
            builder.RegisterType<AlgoTradesSubscriber>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.AlgoTradesJob.Rabbit));
        }

        private void RegisterApplicationServices(ContainerBuilder builder)
        {
            builder.RegisterType<AlgoTradesHistoryWriter>()
                .As<IAlgoTradesHistoryWriter>()
                .SingleInstance();
        }

        private static AlgoInstanceTradeRepository CreateAlgoTradeRepository(IReloadingManager<string> connectionString,
            ILog log)
        {
            return new AlgoInstanceTradeRepository(
                AzureTableStorage<AlgoInstanceTradeEntity>.Create(connectionString, AlgoInstanceTradeRepository.TableName, log));
        }
    }
}
