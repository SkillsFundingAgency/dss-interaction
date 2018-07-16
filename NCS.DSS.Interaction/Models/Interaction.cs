using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Interaction.Annotations;
using NCS.DSS.Interaction.ReferenceData;

namespace NCS.DSS.Interaction.Models
{
    public class Interaction
    {
        [Display(Description = "Unique identifier for the interaction record.")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? InteractionId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of a customer.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? CustomerId { get; set; }

        [Required]
        [Display(Description = "Unique identifier for the touchpoint with which the interaction took place.")]
        [Example(Description = "f823d23a-4006-4572-aef5-65ff085b4687")]
        public Guid? TouchpointId { get; set; }

        [Display(Description = "Unique identifier of the adviser involved in the interaction.")]
        [Example(Description = "6eed4005-4364-4bcb-affb-170ee402d1aa")]
        public Guid? AdviserDetailsId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the interaction took place")]
        [Example(Description = "2018-06-21T11:21:00")]
        public DateTime? DateandTimeOfInteraction { get; set; }

        [Display(Description = "Channel reference data")]
        [Example(Description = "1")]
        public Channel Channel { get; set; }

        [Display(Description = "Business event reference data")]
        [Example(Description = "2")]
        public BusinessEvent BusinessEvent { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-22T16:52:10")]
        public DateTime? LastModifiedDate { get; set; }
        
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "d1307d77-af23-4cb4-b600-a60e04f8c3df")]
        public Guid? LastModifiedTouchpointId { get; set; }

        public void Patch(InteractionPatch interactionPatch)
        {
            if (interactionPatch == null)
                return;

            if(interactionPatch.TouchpointId.HasValue)
                TouchpointId = interactionPatch.TouchpointId;

            if (interactionPatch.AdviserDetailsId.HasValue)
                AdviserDetailsId = interactionPatch.AdviserDetailsId;

            if (interactionPatch.DateandTimeOfInteraction.HasValue)
                DateandTimeOfInteraction = interactionPatch.DateandTimeOfInteraction;

            if(interactionPatch.Channel.HasValue)
                Channel = interactionPatch.Channel.Value;

            if (interactionPatch.BusinessEvent.HasValue)
                BusinessEvent = interactionPatch.BusinessEvent.Value;

            if (interactionPatch.LastModifiedDate.HasValue)
                LastModifiedDate = interactionPatch.LastModifiedDate;

            if (interactionPatch.LastModifiedTouchpointId.HasValue)
                LastModifiedTouchpointId = interactionPatch.LastModifiedTouchpointId;
        }
    }
}