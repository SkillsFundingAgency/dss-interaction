using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Provider;
using NCS.DSS.Interaction.Models;
using NCS.DSS.Interaction.ServiceBus;
using System.Net;

namespace NCS.DSS.Interaction.PatchInteractionHttpTrigger.Service
{
    public class PatchInteractionHttpTriggerService : IPatchInteractionHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly IInteractionServiceBusClient _interactionServiceBusClient;
        private readonly ILogger<PatchInteractionHttpTriggerService> _logger;

        public PatchInteractionHttpTriggerService(ICosmosDbProvider cosmosDbProvider, IInteractionServiceBusClient interactionServiceBusClient, ILogger<PatchInteractionHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _interactionServiceBusClient = interactionServiceBusClient;
            _logger = logger;
        }

        public async Task<Models.Interaction> UpdateAsync(Models.Interaction interaction, InteractionPatch interactionPatch)
        {
            if (interaction == null)
            {
                _logger.LogInformation("The interaction object provided is null.");
                return null;
            }

            interactionPatch.SetDefaultValues();

            _logger.LogInformation("Patching interaction with ID: {InteractionId}.", interaction.InteractionId);
            interaction.Patch(interactionPatch);


            var response = await _cosmosDbProvider.UpdateInteractionAsync(interaction);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Successfully updated interaction with ID: {InteractionId}.", interaction.InteractionId);
                return interaction;
            }
            else
            {
                _logger.LogError("Failed to update interaction with ID: {InteractionId}. Status Code: {StatusCode}.",
                    interaction.InteractionId, response.StatusCode);
                return null;
            }
        }

        public async Task<Models.Interaction> GetInteractionForCustomerAsync(Guid customerId, Guid interactionId)
        {
            _logger.LogInformation("Retrieving interaction with ID: {InteractionId} for customer ID: {CustomerId}.", interactionId, customerId);

            var interaction = await _cosmosDbProvider.GetInteractionForCustomerAsync(customerId, interactionId);

            if (interaction == null)
            {
                _logger.LogWarning("No interaction found with ID: {InteractionId} for customer ID: {CustomerId}.", interactionId, customerId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved interaction with ID: {InteractionId} for customer ID: {CustomerId}.", interactionId, customerId);
            }

            return interaction;
        }

        public async Task SendToServiceBusQueueAsync(Models.Interaction interaction, Guid customerId, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Sending interaction with ID: {InteractionId} to Service Bus for customer ID: {CustomerId}.", interaction.InteractionId, customerId);

                await _interactionServiceBusClient.SendPatchMessageAsync(interaction, customerId, reqUrl);

                _logger.LogInformation("Successfully sent interaction with ID: {InteractionId} to Service Bus for customer ID: {CustomerId}.", interaction.InteractionId, customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending interaction with ID: {InteractionId} to Service Bus for customer ID: {CustomerId}.", interaction.InteractionId, customerId);
                throw;
            }
        }
    }
}