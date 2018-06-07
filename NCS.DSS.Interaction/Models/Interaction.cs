using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Interaction.Models
{
    public class Interaction
    {
        public Guid InteractionId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid TouchpointId { get; set; }

        public Guid AdviserDetailsId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateandTimeOfInteraction { get; set; }

        public int ChannelId { get; set; }

        public int BusinessEventId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastModifiedDate { get; set; }

        public Guid LastModifiedTouchpointId { get; set; }
    }
}