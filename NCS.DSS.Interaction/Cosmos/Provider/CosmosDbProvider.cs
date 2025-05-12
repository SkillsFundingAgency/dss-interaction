using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Interaction.Models;

namespace NCS.DSS.Interaction.Cosmos.Provider
{
    public class CosmosDbProvider : ICosmosDbProvider
    {
        private readonly Container _interactionContainer;
        private readonly Container _customerContainer;
        private readonly ILogger<CosmosDbProvider> _logger;

        public CosmosDbProvider(CosmosClient cosmosClient,
            IOptions<InteractionConfigurationSettings> configOptions,
            ILogger<CosmosDbProvider> logger)
        {
            var config = configOptions.Value;

            _interactionContainer = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
            _customerContainer = GetContainer(cosmosClient, config.CustomerDatabaseId, config.CustomerCollectionId);
            _logger = logger;
        }

        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
            => cosmosClient.GetContainer(databaseId, collectionId);

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Checking for customer resource. Customer ID: {CustomerId}", customerId);

                var response = await _customerContainer.ReadItemAsync<Customer>(
                    customerId.ToString(),
                    PartitionKey.None);

                if (response.Resource != null)
                {
                    _logger.LogInformation("Customer exists. Customer ID: {CustomerId}", customerId);
                    return true;
                }

                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer resource existence. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            _logger.LogInformation("Checking for termination date. Customer ID: {CustomerId}", customerId);

            try
            {
                var response = await _customerContainer.ReadItemAsync<Customer>(
                    customerId.ToString(),
                    PartitionKey.None);

                var dateOfTermination = response.Resource?.DateOfTermination;
                var hasTerminationDate = dateOfTermination != null;

                _logger.LogInformation("Termination date check completed. CustomerId: {CustomerId}. HasTerminationDate: {HasTerminationDate}", customerId, hasTerminationDate);
                return hasTerminationDate;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If a 404 occurs, the resource does not exist
                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking termination date. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<Models.Interaction> GetInteractionAsync(Guid interactionId)
        {
            _logger.LogInformation("Retrieving Interaction. Interaction ID: {InteractionId}", interactionId);

            try
            {
                var response = await _interactionContainer.ReadItemAsync<Models.Interaction>(
                    interactionId.ToString(),
                    PartitionKey.None);

                if (response?.Resource != null)
                {
                    _logger.LogInformation("Interaction retrieved successfully. Interaction ID: {InertactionId}", interactionId);
                    return response.Resource;
                }

                _logger.LogWarning("Interaction not found. Interaction ID: {InertactionId}", interactionId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving for Interaction ID: {interactionId}", interactionId);
                throw;
            }
        }

        public async Task<Models.Interaction> GetInteractionForCustomerAsync(Guid customerId, Guid interactionId)
        {
            _logger.LogInformation("Retrieving Interaction for Customer. Customer ID: {CustomerId}. Interaction ID: {InteractionId}", customerId, interactionId);

            try
            {
                var query = _interactionContainer.GetItemLinqQueryable<Models.Interaction>()
                    .Where(x => x.CustomerId == customerId && x.InteractionId == interactionId)
                    .ToFeedIterator();

                var response = await query.ReadNextAsync();
                if (response.Any())
                {
                    _logger.LogInformation("Interaction retrieved successfully. Customer ID: {CustomerId}. Interaction ID: {InteractionId}", customerId, interactionId);
                    return response.FirstOrDefault();
                }

                _logger.LogWarning("Interaction not found. Interaction ID: {InertactionId}", interactionId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving Interaction. Customer ID: {CustomerId}. Interaction ID: {InteractionId}", customerId, interactionId);
                throw;
            }
        }

        public async Task<List<Models.Interaction>> GetInteractionsForCustomerAsync(Guid customerId)
        {
            _logger.LogInformation("Retrieving interactions for customer. Customer ID: {CustomerId}.", customerId);

            try
            {
                var interactions = new List<Models.Interaction>();
                var query = _interactionContainer
                    .GetItemLinqQueryable<Models.Interaction>()
                    .Where(x => x.CustomerId == customerId)
                    .ToFeedIterator();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    interactions.AddRange(response);
                }

                _logger.LogInformation("Retrieved {Count} interaction(s) for Customer ID: {CustomerId}.", interactions.Count, customerId);
                return interactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving interactions for Customer ID: {CustomerId}.", customerId);
                throw;
            }
        }

        public async Task<ItemResponse<Models.Interaction>> CreateInteractionAsync(Models.Interaction interaction)
        {
            if (interaction == null)
            {
                _logger.LogWarning("Interaction object is null. Creation aborted.");
                throw new ArgumentNullException(nameof(interaction), "Interaction cannot be null.");
            }

            _logger.LogInformation("Creating Interaction with ID: {InteractionId}", interaction.InteractionId);

            try
            {
                var response = await _interactionContainer.CreateItemAsync(interaction, PartitionKey.None);
                _logger.LogInformation("Successfully created Interaction with ID: {InteractionId}", interaction.InteractionId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Interaction with ID: {InteractionId}", interaction.InteractionId);
                throw;
            }
        }

        public async Task<ItemResponse<Models.Interaction>> UpdateInteractionAsync(Models.Interaction interaction)
        {
            if (interaction == null)
            {
                _logger.LogWarning("Interaction object is null. Update aborted.");
                throw new ArgumentNullException(nameof(interaction), "Interaction cannot be null.");
            }

            _logger.LogInformation("Updating Interaction with ID: {InteractionId}", interaction.InteractionId);

            try
            {
                var response = await _interactionContainer.ReplaceItemAsync(interaction, interaction.InteractionId.ToString());
                _logger.LogInformation("Successfully updated Interaction with ID: {InteractionId}", interaction.InteractionId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Interaction with ID: {InteractionId}", interaction.InteractionId);
                throw;
            }
        }
    }
}