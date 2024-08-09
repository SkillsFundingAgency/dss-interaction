using NCS.DSS.Interaction.Cosmos.Provider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.GetInteractionHttpTrigger.Service
{
    public class GetInteractionHttpTriggerService : IGetInteractionHttpTriggerService
    {
        public async Task<List<Models.Interaction>> GetInteractionsAsync(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var interactions = await documentDbProvider.GetInteractionsForCustomerAsync(customerId);

            return interactions;
        }
    }
}
