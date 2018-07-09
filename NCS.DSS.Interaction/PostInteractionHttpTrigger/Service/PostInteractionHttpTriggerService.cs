using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Interaction.Cosmos.Provider;

namespace NCS.DSS.Interaction.PostInteractionHttpTrigger.Service
{
    public class PostInteractionHttpTriggerService : IPostInteractionHttpTriggerService
    {
        public async Task<Guid?> CreateAsync(Models.Interaction interaction)
        {
            if (interaction == null)
                return null;

            var interactionId = Guid.NewGuid();
            interaction.InteractionId = interactionId;

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateInteractionAsync(interaction);

            return response.StatusCode == HttpStatusCode.Created ? interactionId : (Guid?)null;
        }
    }
}