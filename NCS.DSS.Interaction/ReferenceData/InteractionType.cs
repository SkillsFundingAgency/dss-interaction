using System.ComponentModel;

namespace NCS.DSS.Interaction.ReferenceData
{
    public enum InteractionType
    {
        [Description("Transfer to touchpoint")]
        TransferToTouchPoint = 1,

        [Description("Webchat")]
        WebChat = 2,

        [Description("Book an appointment")]
        BookAnAppointment = 3,

        [Description("Creation of actionplan")]
        CreationOfActionPlan = 4,

        [Description("Telephone call")]
        TelephoneCall = 5,

        [Description("Request to be contacted")]
        RequestToBeContacted = 6,

        [Description("Request for technical help")]
        RequestForTechnicalHelp = 7,

        [Description("Provides feedback")]
        ProvidesFeedback = 8,

        [Description("Complaint")]
        Complaint = 9,

        [Description("Voice of customer survey")]
        VoiceOfCustomerSurvey = 10,

        [Description("Other")]
        Other = 99

    }
}
