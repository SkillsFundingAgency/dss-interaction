using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Interaction.Annotations;
using NCS.DSS.Interaction.ReferenceData;

namespace NCS.DSS.Interaction.Models
{
    public class Interaction : IInteraction
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

        [Required]
        [Display(Description = "Channel reference data")]
        [Example(Description = "1")]
        public Channel? Channel { get; set; }

        [Required]
        [Display(Description = "Interaction Type reference data")]
        [Example(Description = "2")]
        public InteractionType? InteractionType { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-22T16:52:10")]
        public DateTime? LastModifiedDate { get; set; }
        
        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointId { get; set; }

        public void SetDefaultValues()
        {
            InteractionId = Guid.NewGuid();

            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;
        }

        public void Patch(InteractionPatch interactionPatch)
        {
            if (interactionPatch == null)
                return;

            if (interactionPatch.AdviserDetailsId.HasValue)
                AdviserDetailsId = interactionPatch.AdviserDetailsId;

            if (interactionPatch.DateandTimeOfInteraction.HasValue)
                DateandTimeOfInteraction = interactionPatch.DateandTimeOfInteraction;

            if(interactionPatch.Channel.HasValue)
                Channel = interactionPatch.Channel.Value;

            if (interactionPatch.InteractionType.HasValue)
                InteractionType = interactionPatch.InteractionType.Value;

            if (interactionPatch.LastModifiedDate.HasValue)
                LastModifiedDate = interactionPatch.LastModifiedDate;

            if (!string.IsNullOrEmpty(interactionPatch.LastModifiedTouchpointId))
                LastModifiedTouchpointId = interactionPatch.LastModifiedTouchpointId;
        }
    }
}