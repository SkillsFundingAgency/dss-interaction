using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Interaction.ReferenceData;

namespace NCS.DSS.Interaction.GetInteractionHttpTrigger
{
    public class GetInteractionHttpTriggerService
    {
        public async Task<List<Models.Interaction>> GetInteractions()
        {
            var result = CreateTempInteractions();
            return await Task.FromResult(result);
        }

        public List<Models.Interaction> CreateTempInteractions()
        {
            var interactionList = new List<Models.Interaction>
            {
                new Models.Interaction
                {
                    InteractionId = Guid.Parse("72888979-3eb0-4e32-87b0-996464d1c23e"),
                    CustomerId = Guid.NewGuid(),
                    TouchpointId = Guid.NewGuid(),
                    AdviserDetailsId = Guid.NewGuid(),
                    DateandTimeOfInteraction = DateTime.Today,
                    Channel = Channel.Cobrowse,
                    InteractionType = InteractionType.BookAnAppointment,
                    LastModifiedDate = DateTime.Today.AddYears(-1),
                    LastModifiedTouchpointId = Guid.NewGuid()
                },
                new Models.Interaction
                {
                    InteractionId = Guid.Parse("2c8391b4-e017-421f-b0e6-7fc5cfec9861"),
                    CustomerId = Guid.NewGuid(),
                    TouchpointId = Guid.NewGuid(),
                    AdviserDetailsId = Guid.NewGuid(),
                    DateandTimeOfInteraction = DateTime.Today,
                    Channel = Channel.Email,
                    InteractionType = InteractionType.TransferToTouchpoint,
                    LastModifiedDate = DateTime.Today.AddYears(-2),
                    LastModifiedTouchpointId = Guid.NewGuid()
                },
                new Models.Interaction
                {
                    InteractionId = Guid.Parse("a2bc68f4-7ffc-4679-bd1b-ad9329e41b14"),
                    CustomerId = Guid.NewGuid(),
                    TouchpointId = Guid.NewGuid(),
                    AdviserDetailsId = Guid.NewGuid(),
                    DateandTimeOfInteraction = DateTime.Today,
                    Channel = Channel.Other,
                    InteractionType = InteractionType.Other,
                    LastModifiedDate = DateTime.Today.AddYears(-3),
                    LastModifiedTouchpointId = Guid.NewGuid()
                }
            };

            return interactionList;
        }

    }
}
