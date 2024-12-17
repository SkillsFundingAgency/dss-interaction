using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Provider;

namespace NCS.DSS.Interaction.GetInteractionHttpTrigger.Service
{
    public class GetInteractionHttpTriggerService : IGetInteractionHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly ILogger<GetInteractionHttpTriggerService> _logger;

        public GetInteractionHttpTriggerService(ICosmosDbProvider cosmosDbProvider, ILogger<GetInteractionHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _logger = logger;
        }

        public async Task<List<Models.Interaction>> GetInteractionsAsync(Guid customerId)
        {
            _logger.LogInformation("Retrieving interactions for customer ID: {CustomerId}.", customerId);

            if (customerId == Guid.Empty)
            {
                _logger.LogWarning("Invalid customer ID provided: {CustomerId}.", customerId);
                return null;
            }

            var interactions = await _cosmosDbProvider.GetInteractionsForCustomerAsync(customerId);

            if (interactions == null)
            {
                _logger.LogInformation("No interactions found for customer ID: {CustomerId}.", customerId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved interaction(s) for customer ID: {CustomerId}.", customerId);
            }

            return interactions;
        }
    }
}
