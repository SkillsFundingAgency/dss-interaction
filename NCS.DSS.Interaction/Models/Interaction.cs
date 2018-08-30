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

        [Display(Description = "Unique identifier of a customer.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? CustomerId { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Unique identifier for the touchpoint with which the interaction took place.")]
        [Example(Description = "0000000001")]
        public string TouchpointId { get; set; }

        [Display(Description = "Unique identifier of the adviser involved in the interaction.")]
        [Example(Description = "6eed4005-4364-4bcb-affb-170ee402d1aa")]
        public Guid? AdviserDetailsId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the interaction took place")]
        [Example(Description = "2018-06-21T11:21:00")]
        public DateTime? DateandTimeOfInteraction { get; set; }

        [Required]
        [Display(Description = "Channel reference data   :   " +
                                "1 - Face to face,  " +
                                "2 - Telephone,  " +
                                "3 - Webchat,  " +
                                "4 - Videochat,  " +
                                "5 - Email,  " +
                                "6 - Social media,  " +
                                "7 - SMS,  " +
                                "8 - Post,  " +
                                "9 - Co - browse,  "  + 
                                "99 - Other")]
        [Example(Description = "1")]
        public Channel? Channel { get; set; }

        [Required]
        [Display(Description = "Interaction Type reference data   :   " +
                                "1 - Transfer to touchpoint,   " +
                                "2 - WebChat,   " +
                                "3 - Book an appointment,   " +
                                "4 - Creation of an action plan,   " +
                                "5 - Telephone call,   " +
                                "6 - Request to be contacted,   " +
                                "7 - Request for technical help,   " +
                                "8 - Provides feedback,   " +
                                "9 - Complaint,   " +
                                "10 - Voice of customer survey,   " + 
                                "99 - Other ")]
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

            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;
        }

        public void SetIds(Guid customerId, string touchpointId)
        {
            InteractionId = Guid.NewGuid();
            CustomerId = customerId;
            TouchpointId = touchpointId;
            LastModifiedTouchpointId = touchpointId;
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