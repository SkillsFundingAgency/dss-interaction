using System.ComponentModel;

namespace NCS.DSS.Interaction.ReferenceData
{
    public enum InteractionType
    {
        [Description("Transfer to touchpoint")]
        TransferToTouchpoint = 1,
        WebChat = 2,
        [Description("Book an appointment")]
        BookAnAppointment = 3,
        [Description("Creation of an action plan")]
        CreationOfAnActionPlan = 4,
        [Description("Telephone call")]
        TelephoneCall = 5,
        [Description("Request to be contacted")]
        RequestToBeContacted = 6,
        [Description("Request for technical help")]
        RequestForTechnicalHelp = 7,
        [Description("Provides feedback")]
        ProvidesFeedback = 8,
        Other = 99
    }
}
