using System;
using NCS.DSS.Interaction.ReferenceData;

namespace NCS.DSS.Interaction.Models
{
    public interface IInteraction
    {
        Guid? AdviserDetailsId { get; set; }
        DateTime? DateandTimeOfInteraction { get; set; }
        Channel? Channel { get; set; }
        InteractionType? InteractionType { get; set; }
        DateTime? LastModifiedDate { get; set; }
        Guid? LastModifiedTouchpointId { get; set; }

        void SetDefaultValues();
        
    }
}