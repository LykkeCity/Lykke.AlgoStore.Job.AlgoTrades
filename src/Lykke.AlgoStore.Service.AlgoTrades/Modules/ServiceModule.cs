using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.AlgoStore.Service.AlgoTrades.Services;
using Lykke.AlgoStore.Service.AlgoTrades.Settings;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using AzureStorage.Tables;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Entities;
using Lykke.Common.Log;

namespace Lykke.AlgoStore.Service.AlgoTrades.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterRepositories(builder);

            RegisterApplicationServices(builder);

            RegisterDictionaryEntities(builder);

            builder.RegisterAssetsClient(AssetServiceSettings.Create(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl), TimeSpan.FromMinutes(3)));
            
            builder.Populate(_services);
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            builder.Register(x =>
                {
                    var log = x.Resolve<ILogFactory>();
                    var repository = CreateAlgoTradeRepository(
                        _settings.Nested(y => y.AlgoTradesService.Db.LogsConnString), log);

                    return repository;
                })
                .As<IAlgoInstanceTradeRepository>()
                .SingleInstance();
        }

        private void RegisterApplicationServices(ContainerBuilder builder)
        {
            builder.RegisterType<AlgoInstanceTradesHistoryService>()
                .As<IAlgoInstanceTradesHistoryService>()
                .SingleInstance();
        }

        private void RegisterDictionaryEntities(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var ctx = c.Resolve<IComponentContext>();
                return new CachedDataDictionary<string, Asset>(
                    async () =>
                        (await ctx.Resolve<IAssetsService>().AssetGetAllAsync()).ToDictionary(itm => itm.Id));
            }).SingleInstance();

            builder.Register(c =>
            {
                var ctx = c.Resolve<IComponentContext>();
                return new CachedDataDictionary<string, AssetPair>(
                    async () =>
                        (await ctx.Resolve<IAssetsService>().AssetPairGetAllAsync())
                        .ToDictionary(itm => itm.Id));
            }).SingleInstance();
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
