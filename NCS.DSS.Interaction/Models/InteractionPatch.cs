using System;
using System.ComponentModel.DataAnnotations;
using DFC.Swagger.Standard.Annotations;
using NCS.DSS.Interaction.ReferenceData;

namespace NCS.DSS.Interaction.Models
{
    public class InteractionPatch : IInteraction
    {

        [Display(Description = "Unique identifier of the adviser involved in the interaction.")]
        [Example(Description = "6eed4005-4364-4bcb-affb-170ee402d1aa")]
        public Guid? AdviserDetailsId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time the interaction took place")]
        [Example(Description = "2018-06-21T11:21:00")]
        public DateTime? DateandTimeOfInteraction { get; set; }

        [Display(Description = "Channel reference data   :   " +
                                "1 - Face to face,  " +
                                "2 - Telephone,  " +
                                "3 - Webchat,  " +
                                "4 - Videochat,  " +
                                "5 - Email,  " +
                                "6 - Social media,  " +
                                "7 - SMS,  " +
                                "8 - Post,  " +
                                "9 - Co - browse,  " +
                                "99 - Other")]
        [Example(Description = "1")]
        public Channel? Channel { get; set; }

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
        [RegularExpression(@"^[0-9]+$")]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointId { get; set; }

        public void SetDefaultValues()
        {
            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;
        }
    }
}
