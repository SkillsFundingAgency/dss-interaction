namespace NCS.DSS.Interaction.PatchInteractionHttpTrigger.Service
{
    public interface IPatchInteractionHttpTriggerService
    {
        Task<Models.Interaction> UpdateAsync(Models.Interaction interaction, Models.InteractionPatch interactionPatch);
        Task<Models.Interaction> GetInteractionForCustomerAsync(Guid customerId, Guid interactionId);
        Task SendToServiceBusQueueAsync(Models.Interaction interaction, Guid customerId, string reqUrl);
    }
}