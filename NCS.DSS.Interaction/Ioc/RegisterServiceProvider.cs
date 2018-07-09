using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service;
using NCS.DSS.Interaction.GetInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Helpers;
using NCS.DSS.Interaction.PatchInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.PostInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Validation;


namespace NCS.DSS.Interaction.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IGetInteractionHttpTriggerService, GetInteractionHttpTriggerService>();
            services.AddTransient<IGetInteractionByIdHttpTriggerService, GetInteractionByIdHttpTriggerService>();
            services.AddTransient<IPatchInteractionHttpTriggerService, PatchInteractionHttpTriggerService>();
            services.AddTransient<IPostInteractionHttpTriggerService, PostInteractionHttpTriggerService>();
            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            return services.BuildServiceProvider(true);
        }
    }
}
