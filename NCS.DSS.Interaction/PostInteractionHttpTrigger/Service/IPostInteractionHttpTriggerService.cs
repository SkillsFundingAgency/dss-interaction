using System.Threading.Tasks;

namespace NCS.DSS.Interaction.PostInteractionHttpTrigger.Service
{
    public interface IPostInteractionHttpTriggerService
    {
        Task<Models.Interaction> CreateAsync(Models.Interaction interaction);
        Task SendToServiceBusQueueAsync(Models.Interaction interaction, string reqUrl);
    }
}