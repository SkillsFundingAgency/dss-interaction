using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.GetInteractionHttpTrigger.Service
{
    public interface IGetInteractionHttpTriggerService
    {
        Task<List<Models.Interaction>> GetInteractionsAsync(Guid customerId);
    }
}