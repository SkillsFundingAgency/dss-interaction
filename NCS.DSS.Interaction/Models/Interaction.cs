using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Interaction.Models
{
    public class Interaction
    {
        [Display(Description = "Unique identifier for the interaction record.")]
        public Guid InteractionId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of a customer.")]
        public Guid CustomerId { get; set; }

        [Required]
        [Display(Description = "Unique identifier for the touchpoint with which the interaction took place.")]
        public Guid TouchpointId { get; set; }

        [Display(Description = "Unique identifier of the adviser involved in the interaction.")]
        public Guid AdviserDetailsId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the interaction took place")]
        public DateTime DateandTimeOfInteraction { get; set; }

        [Display(Description = "Channel reference data")]
        public int ChannelId { get; set; }

        [Display(Description = "Business event reference data")]
        public int BusinessEventId { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        public DateTime LastModifiedDate { get; set; }
        
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        public Guid LastModifiedTouchpointId { get; set; }
    }
}