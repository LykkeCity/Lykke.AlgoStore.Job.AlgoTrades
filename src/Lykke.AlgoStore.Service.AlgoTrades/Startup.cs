using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Common.Log;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Mapper;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.AlgoStore.Service.AlgoTrades.Infrastructure;
using Lykke.AlgoStore.Service.AlgoTrades.Settings;
using Lykke.AlgoStore.Service.AlgoTrades.Modules;
using Lykke.Common;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.AlgoStore.Service.AlgoTrades
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }
        private ILog _log;

        public Startup(IHostingEnvironment env)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfiles(typeof(AutoMapperModelProfile));
                cfg.AddProfile(typeof(HistoryAutoMapperModelProfile));
            });

            Mapper.AssertConfigurationIsValid();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver =
                            new Newtonsoft.Json.Serialization.DefaultContractResolver();
                    });

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", "AlgoTrades API");
                    options.SchemaFilter<NullableTypeSchemaFilter>();
                });

                var settingsManager = Configuration.LoadSettings<AppSettings>(x => (
                    x.SlackNotifications.AzureQueue.ConnectionString, x.SlackNotifications.AzureQueue.QueueName,
                    $"{AppEnvironment.Name} {AppEnvironment.Version}"));

                var appSettings = settingsManager.CurrentValue;

                services.AddLykkeLogging(
                    settingsManager.ConnectionString(s => s.AlgoTradesService.Db.LogsConnString),
                    "AlgoTradesLog",
                    appSettings.SlackNotifications.AzureQueue.ConnectionString,
                    appSettings.SlackNotifications.AzureQueue.QueueName);

                var builder = new ContainerBuilder();
                
                builder.RegisterModule(new ServiceModule(settingsManager));
                builder.Populate(services);

                ApplicationContainer = builder.Build();

                var logFactory = ApplicationContainer.Resolve<ILogFactory>();
                _log = logFactory.CreateLog(this);

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                _log.Critical(nameof(Startup), ex, nameof(ConfigureServices));
                throw;
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseLykkeForwardedHeaders();
                app.UseLykkeMiddleware(ex => new ErrorResponse { ErrorMessage = "Technical problem" });

                app.UseMvc();
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });
                app.UseStaticFiles();

                appLifetime.ApplicationStarted.Register(StartApplication);
                appLifetime.ApplicationStopping.Register(StopApplication);
                appLifetime.ApplicationStopped.Register(CleanUp);
            }
            catch (Exception ex)
            {
                _log?.Critical(nameof(Startup), ex, nameof(Configure));
                throw;
            }
        }

        private void StartApplication()
        {
            try
            {
                _log?.Info(nameof(StartApplication), "Started", $"Env: {Program.EnvInfo}");
            }
            catch (Exception ex)
            {
                _log?.Critical(nameof(StartApplication), ex);
                throw;
            }
        }

        private void StopApplication()
        {
            try
            {
                _log?.Info(nameof(StopApplication), "Stopped", $"Env: {Program.EnvInfo}");
            }
            catch (Exception ex)
            {
                _log?.Critical(nameof(StopApplication), ex);
                throw;
            }
        }

        private void CleanUp()
        {
            try
            {
                _log?.Info(nameof(CleanUp), "Terminating", $"Env: {Program.EnvInfo}");

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log.Critical(nameof(CleanUp), ex, "", nameof(CleanUp));

                    (_log as IDisposable)?.Dispose();
                }
                throw;
            }
        }
    }
}
