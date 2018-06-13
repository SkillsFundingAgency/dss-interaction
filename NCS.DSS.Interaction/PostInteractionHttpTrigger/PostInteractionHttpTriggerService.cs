using System;

namespace NCS.DSS.Interaction.PostInteractionHttpTrigger
{
    public class PostInteractionHttpTriggerService
    {
        public Guid? Create(Models.Interaction interaction)
        {
            if (interaction == null)
                return null;

            return Guid.NewGuid();
        }
    }
}