using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Interaction;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service;
using NCS.DSS.Interaction.GetInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.PatchInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.PostInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Validation;

[assembly: FunctionsStartup(typeof(Startup))]
namespace NCS.DSS.Interaction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddTransient<IGetInteractionHttpTriggerService, GetInteractionHttpTriggerService>();
            builder.Services.AddTransient<IGetInteractionByIdHttpTriggerService, GetInteractionByIdHttpTriggerService>();
            builder.Services.AddTransient<IPatchInteractionHttpTriggerService, PatchInteractionHttpTriggerService>();
            builder.Services.AddTransient<IPostInteractionHttpTriggerService, PostInteractionHttpTriggerService>();
            builder.Services.AddTransient<IResourceHelper, ResourceHelper>();
            builder.Services.AddTransient<IValidate, Validate>();
        }
    }
}
