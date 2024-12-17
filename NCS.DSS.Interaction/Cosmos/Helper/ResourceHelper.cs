using NCS.DSS.Interaction.Cosmos.Provider;

namespace NCS.DSS.Interaction.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;

        public ResourceHelper(ICosmosDbProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }
        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            var doesCustomerExist = await _cosmosDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var isCustomerReadOnly = await _cosmosDbProvider.DoesCustomerHaveATerminationDate(customerId);

            return isCustomerReadOnly;
        }
    }
}
