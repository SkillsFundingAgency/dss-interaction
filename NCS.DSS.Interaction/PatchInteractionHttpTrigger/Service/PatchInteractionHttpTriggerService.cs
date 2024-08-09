using NCS.DSS.Interaction.Cosmos.Provider;
using NCS.DSS.Interaction.Models;
using NCS.DSS.Interaction.ServiceBus;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.PatchInteractionHttpTrigger.Service
{
    public class PatchInteractionHttpTriggerService : IPatchInteractionHttpTriggerService
    {
        public async Task<Models.Interaction> UpdateAsync(Models.Interaction interaction, InteractionPatch interactionPatch)
        {
            if (interaction == null)
                return null;

            interactionPatch.SetDefaultValues();

            interaction.Patch(interactionPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateInteractionAsync(interaction);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? interaction : null;

        }

        public async Task<Models.Interaction> GetInteractionForCustomerAsync(Guid customerId, Guid interactionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var interaction = await documentDbProvider.GetInteractionForCustomerAsync(customerId, interactionId);

            return interaction;
        }

        public async Task SendToServiceBusQueueAsync(Models.Interaction interaction, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(interaction, customerId, reqUrl);
        }
    }
}