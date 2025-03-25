using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Provider;

namespace NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service
{
    public class GetInteractionByIdHttpTriggerService : IGetInteractionByIdHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly ILogger<GetInteractionByIdHttpTriggerService> _logger;
        public GetInteractionByIdHttpTriggerService(ICosmosDbProvider cosmosDbProvider, ILogger<GetInteractionByIdHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _logger = logger;
        }

        public async Task<Models.Interaction> GetInteractionForCustomerAsync(Guid customerId, Guid interactionId)
        {
            _logger.LogInformation("Retrieving interaction with ID: {InteractionId} for customer ID: {CustomerId}.", interactionId, customerId);

            if (customerId == Guid.Empty)
            {
                _logger.LogWarning("Invalid customer ID provided: {CustomerId}.", customerId);
                return null;
            }

            if (interactionId == Guid.Empty)
            {
                _logger.LogWarning("Invalid interaction ID provided: {InteractionId}.", interactionId);
                return null;
            }

            var interaction = await _cosmosDbProvider.GetInteractionForCustomerAsync(customerId, interactionId);

            if (interaction == null)
            {
                _logger.LogInformation("No interaction found with ID: {InteractionId} for customer ID: {CustomerId}.", interactionId, customerId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved interaction with ID: {InteractionId} for customer ID: {CustomerId}.", interactionId, customerId);
            }

            return interaction;
        }
    }
}