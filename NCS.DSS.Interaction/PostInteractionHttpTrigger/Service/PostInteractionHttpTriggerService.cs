using NCS.DSS.Interaction.Cosmos.Provider;
using NCS.DSS.Interaction.ServiceBus;
using System.Net;

namespace NCS.DSS.Interaction.PostInteractionHttpTrigger.Service
{
    public class PostInteractionHttpTriggerService : IPostInteractionHttpTriggerService
    {
        public async Task<Models.Interaction> CreateAsync(Models.Interaction interaction)
        {
            if (interaction == null)
                return null;

            interaction.SetDefaultValues();

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateInteractionAsync(interaction);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Interaction interaction, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(interaction, reqUrl);
        }
    }
}