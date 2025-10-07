using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Provider;
using NCS.DSS.Interaction.ServiceBus;
using System.Net;

namespace NCS.DSS.Interaction.PostInteractionHttpTrigger.Service
{
    public class PostInteractionHttpTriggerService : IPostInteractionHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly IInteractionServiceBusClient _interactionServiceBusClient;
        private readonly ILogger<PostInteractionHttpTriggerService> _logger;

        public PostInteractionHttpTriggerService(ICosmosDbProvider cosmosDbProvider, IInteractionServiceBusClient interactionServiceBusClient, ILogger<PostInteractionHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _interactionServiceBusClient = interactionServiceBusClient;
            _logger = logger;
        }

        public async Task<Models.Interaction> CreateAsync(Models.Interaction interaction)
        {
            if (interaction == null)
            {
                _logger.LogInformation("The interaction object provided is null.");
                return null;
            }

            interaction.SetDefaultValues();

            var response = await _cosmosDbProvider.CreateInteractionAsync(interaction);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                _logger.LogTrace("Successfully created interaction with ID: {InteractionId}.", interaction.InteractionId);
                return response.Resource;
            }
            else
            {
                _logger.LogError("Failed to create interaction with ID: {InteractionId}. Status Code: {StatusCode}.",
                    interaction.InteractionId, response.StatusCode);
                return null;
            }
        }

        public async Task SendToServiceBusQueueAsync(Models.Interaction interaction, string reqUrl)
        {
            try
            {
                _logger.LogTrace("Sending interaction with ID: {InteractionId} to Service Bus.", interaction.InteractionId);

                await _interactionServiceBusClient.SendPostMessageAsync(interaction, reqUrl);

                _logger.LogTrace("Successfully sent interaction with ID: {InteractionId} to Service Bus.", interaction.InteractionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending interaction with ID: {InteractionId} to Service Bus.", interaction.InteractionId);
                throw;
            }
        }
    }
}