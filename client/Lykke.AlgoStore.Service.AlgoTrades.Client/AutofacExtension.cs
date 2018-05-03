using System;
using Autofac;
using Common.Log;

namespace Lykke.AlgoStore.Service.AlgoTrades.Client
{
    public static class AutofacExtension
    {
        public static void RegisterAlgoTradesClient(this ContainerBuilder builder, string serviceUrl, ILog log)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (serviceUrl == null) throw new ArgumentNullException(nameof(serviceUrl));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterType<AlgoTradesClient>()
                .WithParameter("serviceUrl", serviceUrl)
                .As<IAlgoTradesClient>()
                .SingleInstance();
        }

        public static void RegisterAlgoTradesClient(this ContainerBuilder builder, AlgoTradesServiceClientSettings settings, ILog log)
        {
            builder.RegisterAlgoTradesClient(settings?.ServiceUrl, log);
        }
    }
}
