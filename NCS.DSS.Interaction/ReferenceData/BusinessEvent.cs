using System.ComponentModel;

namespace NCS.DSS.Interaction.ReferenceData
{
    public enum BusinessEvent
    {
        [Description("Register a new customer")]
        RegisterANewCustomer = 1,
        [Description("Collection of special category data")]
        CollectionOfSpecialCategoryData = 2,
        [Description("Amend customers details")]
        AmendCustomersDetails = 3,
        [Description("Subscribe to customer changes")]
        SubscribeToCustomerChanges = 4,
        [Description("Merging of a customer record")]
        MergingOfACustomerRecord = 5,
        [Description("Termination of service")]
        TerminationOfService = 6,
        [Description("Warm transfer between touchpoints")]
        WarmTransferBetweenTouchpoints = 7,
        [Description("Cold transfer between touchpoints")]
        ColdTransferBetweenTouchpoints = 8,
        [Description("Creation of a careers and skills action plan")]
        CreationOfACareersAndSkillsActionPlan = 9,
        [Description("Update to a careers and skills action plan")]
        UpdateToACareersAndSkillsActionPlan = 10,
        [Description("Sending a careers and skills action plan to the customer")]
        SendingACareersAndSkillsActionPlanToTheCustomer = 11,
        [Description("Acceptance of a careers and skills action plan by the customer")]
        AcceptanceOfACareersAndSkillsActionPlanByTheCustomer = 12,
        [Description("Update to a action within an action plan")]
        UpdateToAActionWithinAnActionPlan = 13,
        [Description("Book an appointment")]
        BookAnAppointment = 14,
        [Description("Update an appointment")]
        UpdateAnAppointment = 15,
        [Description("Cancel an appointment")]
        CancelAnAppointment = 16,
        [Description("Outcome evidenced")]
        OutcomeEvidenced = 17,
        [Description("Customer having a webchat discussion")]
        CustomerHavingAWebchatDiscussion = 18,
        [Description("Customer requesting webchat narrative")]
        CustomerRequestingWebchatNarrative = 19,
        [Description("Changes in reference data")]
        ChangesInReferenceData = 20,
        [Description("Changes in data schema")]
        ChangesInDataSchema = 21,
        [Description("Customer request for contact / assistance")]
        CustomerRequestForContactAssistance = 22,
        [Description("Understand reason for contact")]
        UnderstandReasonForContact = 23,
        [Description("Data challenge")]
        DataChallenge = 24,
        [Description("Collection of customer feedback")]
        CollectionOfCustomerFeedback = 25,
        [Description("Customer completing pre-appointment questionnaire")]
        CustomerCompletingPreAppointmentQuestionnaire = 26,
        [Description("Determine a customers digital inclusion/capability")]
        DetermineACustomersDigitalInclusionCapability = 27,
        [Description("Send chasing notifications on viewing action plan")]
        SendChasingNotificationsOnViewingActionPlan = 28,
        [Description("DSS Sending a careers and skills action plan to the customer")]
        DSSSendingACareersAndSkillsActionPlanToTheCustomer = 29,
        [Description("Customer using 'find a career' as evidence of a CMO")]
        CustomerUsingFindACareerAsEvidenceOfACMO = 30,
        [Description("Customer using 'find an opportunity' as evidence of a CMO")]
        CustomerUsingFindAnOpportunityAsEvidenceOfACMO = 31,
        [Description("Customer using 'understand myself' as evidence of CMO")]
        CustomerUsingUnderstandMyselfAsEvidenceOfCMO = 32,
        [Description("Customer completing a voice of customer survey")]
        CustomerCompletingAVoiceOfCustomerSurvey = 33,
        [Description("Customer interaction via social media")]
        CustomerInteractionViaSocialMedia = 34,
        [Description("Customer IA interaction via telephone")]
        CustomerIAInteractionViaTelephone = 35

    }
}
