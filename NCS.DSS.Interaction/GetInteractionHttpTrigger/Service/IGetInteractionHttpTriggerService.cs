namespace NCS.DSS.Interaction.GetInteractionHttpTrigger.Service
{
    public interface IGetInteractionHttpTriggerService
    {
        Task<List<Models.Interaction>> GetInteractionsAsync(Guid customerId);
    }
}