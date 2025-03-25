namespace NCS.DSS.Interaction.ServiceBus
{
    public interface IInteractionServiceBusClient
    {
        Task SendPatchMessageAsync(Models.Interaction interaction, Guid customerId, string reqUrl);
        Task SendPostMessageAsync(Models.Interaction interaction, string reqUrl);
    }
}