using Azure.Messaging.ServiceBus;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.Cosmos.Provider;
using NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service;
using NCS.DSS.Interaction.GetInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Helpers;
using NCS.DSS.Interaction.Models;
using NCS.DSS.Interaction.PatchInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.PostInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.ServiceBus;
using NCS.DSS.Interaction.Validation;

namespace NCS.DSS.Outcomes
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    services.AddOptions<InteractionConfigurationSettings>()
                        .Bind(configuration);

                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddLogging();
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                    services.AddTransient<IGetInteractionHttpTriggerService, GetInteractionHttpTriggerService>();
                    services.AddTransient<IGetInteractionByIdHttpTriggerService, GetInteractionByIdHttpTriggerService>();
                    services.AddTransient<IPatchInteractionHttpTriggerService, PatchInteractionHttpTriggerService>();
                    services.AddTransient<IPostInteractionHttpTriggerService, PostInteractionHttpTriggerService>();
                    services.AddTransient<IResourceHelper, ResourceHelper>();
                    services.AddTransient<IValidate, Validate>();
                    services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddSingleton<IDynamicHelper, DynamicHelper>();
                    services.AddSingleton<ICosmosDbProvider, CosmosDbProvider>();
                    services.AddSingleton<IInteractionServiceBusClient, InteractionServiceBusClient>();

                    services.AddSingleton(s =>
                    {
                        var settings = s.GetRequiredService<IOptions<InteractionConfigurationSettings>>().Value;
                        var options = new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway };

                        return new CosmosClient(settings.Endpoint, settings.Key, options);
                    });

                    services.AddSingleton(s =>
                    {
                        var settings = s.GetRequiredService<IOptions<InteractionConfigurationSettings>>().Value;
                        var serviceBusConnectionString = $"Endpoint={settings.BaseAddress};SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={settings.AccessKey}";

                        return new ServiceBusClient(serviceBusConnectionString);
                    });

                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}
