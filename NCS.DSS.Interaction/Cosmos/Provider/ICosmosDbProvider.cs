using Microsoft.Azure.Cosmos;

namespace NCS.DSS.Interaction.Cosmos.Provider
{
    public interface ICosmosDbProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<Models.Interaction> GetInteractionAsync(Guid interactionId);
        Task<Models.Interaction> GetInteractionForCustomerAsync(Guid customerId, Guid interactionId);
        Task<List<Models.Interaction>> GetInteractionsForCustomerAsync(Guid customerId);
        Task<ItemResponse<Models.Interaction>> CreateInteractionAsync(Models.Interaction interaction);
        Task<ItemResponse<Models.Interaction>> UpdateInteractionAsync(Models.Interaction interaction);
    }
}