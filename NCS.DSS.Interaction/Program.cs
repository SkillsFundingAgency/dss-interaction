using DFC.HTTP.Standard;
using DFC.Swagger.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service;
using NCS.DSS.Interaction.GetInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Helpers;
using NCS.DSS.Interaction.PatchInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.PostInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Validation;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        services.AddTransient<IGetInteractionHttpTriggerService, GetInteractionHttpTriggerService>();
        services.AddTransient<IGetInteractionByIdHttpTriggerService, GetInteractionByIdHttpTriggerService>();
        services.AddTransient<IPatchInteractionHttpTriggerService, PatchInteractionHttpTriggerService>();
        services.AddTransient<IPostInteractionHttpTriggerService, PostInteractionHttpTriggerService>();
        services.AddTransient<IResourceHelper, ResourceHelper>();
        services.AddTransient<IValidate, Validate>();
        services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
        services.AddSingleton<IDynamicHelper, DynamicHelper>();
    })
    .Build();

host.Run();
