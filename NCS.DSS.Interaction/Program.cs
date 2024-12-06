using DFC.HTTP.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service;
using NCS.DSS.Interaction.GetInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Helpers;
using NCS.DSS.Interaction.PatchInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.PostInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Validation;

namespace NCS.DSS.Outcomes
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices(services =>
                {
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
