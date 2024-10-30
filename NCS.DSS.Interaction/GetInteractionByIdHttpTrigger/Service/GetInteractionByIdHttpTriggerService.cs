using NCS.DSS.Interaction.Cosmos.Provider;

namespace NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service
{
    public class GetInteractionByIdHttpTriggerService : IGetInteractionByIdHttpTriggerService
    {
        public async Task<Models.Interaction> GetInteractionForCustomerAsync(Guid customerId, Guid interactionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var interaction = await documentDbProvider.GetInteractionForCustomerAsync(customerId, interactionId);

            return interaction;
        }
    }
}