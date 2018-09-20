using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Interaction.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<ResourceResponse<Document>> GetInteractionAsync(Guid interactionId);
        Task<Models.Interaction> GetInteractionForCustomerAsync(Guid customerId, Guid interactionId);
        Task<List<Models.Interaction>> GetInteractionsForCustomerAsync(Guid customerId);
        Task<ResourceResponse<Document>> CreateInteractionAsync(Models.Interaction interaction);
        Task<ResourceResponse<Document>> UpdateInteractionAsync(Models.Interaction interaction);
    }
}