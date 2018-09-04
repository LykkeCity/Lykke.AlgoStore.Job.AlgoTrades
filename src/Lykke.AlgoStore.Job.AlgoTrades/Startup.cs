using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.AlgoStore.Job.AlgoTrades.Modules;
using Lykke.AlgoStore.Job.AlgoTrades.Settings;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using AutoMapper;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Mapper;
using Lykke.Common;
using Lykke.Common.Log;

namespace Lykke.AlgoStore.Job.AlgoTrades
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
                    options.DefaultLykkeConfiguration("v1", "AlgoTrades Job");
                });

                var settingsManager = Configuration.LoadSettings<AppSettings>(x => (
                    x.SlackNotifications.AzureQueue.ConnectionString, x.SlackNotifications.AzureQueue.QueueName,
                    $"{AppEnvironment.Name} {AppEnvironment.Version}"));

                var appSettings = settingsManager.CurrentValue;

                services.AddLykkeLogging(
                    settingsManager.ConnectionString(s => s.AlgoTradesJob.Db.LogsConnString),
                    "AlgoTradesLog",
                    appSettings.SlackNotifications.AzureQueue.ConnectionString,
                    appSettings.SlackNotifications.AzureQueue.QueueName);

                var builder = new ContainerBuilder();

                builder.RegisterModule(new JobModule(settingsManager));

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

                appLifetime.ApplicationStopped.Register(CleanUp);
            }
            catch (Exception ex)
            {
                _log?.Critical(nameof(Startup), ex, nameof(Configure));
                throw;
            }
        }

        private void CleanUp()
        {
            try
            {
                _log?.Info(nameof(CleanUp), "Terminating", Program.EnvInfo);

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
