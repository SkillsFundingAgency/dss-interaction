using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NCS.DSS.Interaction.Cosmos.Provider;

namespace NCS.DSS.Interaction.GetInteractionHttpTrigger.Service
{
    public class GetInteractionHttpTriggerService : IGetInteractionHttpTriggerService
    {
        public async Task<List<Models.Interaction>> GetInteractionsAsync(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var interactions = await documentDbProvider.GetInteractionsForCustomerAsync(customerId);

            return interactions.Any() ? interactions : null;

        }
    }
}
