using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.AlgoStore.Service.AlgoTrades.Settings.ServiceSettings;
using Lykke.AlgoStore.Service.AlgoTrades.Services;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.AlgoStore.Service.AlgoTrades.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AlgoTradesSettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<AlgoTradesSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                  .As<ILog>()
                  .SingleInstance();

            builder.Populate(_services);
        }
    }
}
