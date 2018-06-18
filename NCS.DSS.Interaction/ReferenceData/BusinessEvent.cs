using System.ComponentModel;

namespace NCS.DSS.Interaction.ReferenceData
{
    public enum BusinessEvent
    {
        [Description("Collection of special category data")]
        CollectionOfSpecialCategoryData,
        [Description("Amend customers details")]
        AmendCustomersDetails,
        [Description("Subscribe to customer changes")]
        SubscribeToCustomerChanges,
        [Description("Merging of a customer record")]
        MergingOfACustomerRecord,
        [Description("Termination of service")]
        TerminationOfService,
        [Description("Warm transfer between touchpoints")]
        WarmTransferBetweenTouchpoints,
        [Description("Cold transfer between touchpoints")]
        ColdTransferBetweenTouchpoints,
        [Description("Creation of a careers and skills action plan")]
        CreationOfACareersAndSkillsActionPlan,
        [Description("Update to a careers and skills action plan")]
        UpdateToACareersAndSkillsActionPlan,
        [Description("Sending a careers and skills action plan to the customer")]
        SendingACareersAndSkillsActionPlanToTheCustomer,
        [Description("Acceptance of a careers and skills action plan by the customer")]
        AcceptanceOfACareersAndSkillsActionPlanByTheCustomer,
        [Description("Update to a action within an action plan")]
        UpdateToAActionWithinAnActionPlan,
        [Description("Book an appointment")]
        BookAnAppointment,
        [Description("Update an appointment")]
        UpdateAnAppointment,
        [Description("Cancel an appointment")]
        CancelAnAppointment,
        [Description("Outcome evidenced")]
        OutcomeEvidenced,
        [Description("Customer having a webchat discussion")]
        CustomerHavingAWebchatDiscussion,
        [Description("Customer requesting webchat narrative")]
        CustomerRequestingWebchatNarrative,
        [Description("Changes in reference data")]
        ChangesInReferenceData,
        [Description("Changes in data schema")]
        ChangesInDataSchema,
        [Description("Understand reason for contact")]
        UnderstandReasonForContact,
        [Description("Collection of customer feedback")]
        CollectionOfCustomerFeedback,
        [Description("Customer completing pre-appointment questionnaire")]
        CustomerCompletingPreAppointmentQuestionnaire,
        [Description("Determine a customers digital inclusion/capability")]
        DetermineACustomersDigitalInclusionCapability,
        [Description("Send chasing notifications on viewing action plan")]
        SendChasingNotificationsOnViewingActionPlan,
        [Description("DSS Sending a careers and skills action plan to the customer")]
        DSSSendingACareersAndSkillsActionPlanToTheCustomer,
        [Description("Customer using 'find a career' as evidence of a CMO")]
        CustomerUsingFindACareerAsEvidenceOfACMO,
        [Description("Customer using 'find an opportunity' as evidence of a CMO")]
        CustomerUsingFindAnOpportunityAsEvidenceOfACMO,
        [Description("Customer using 'understand myself' as evidence of CMO")]
        CustomerUsingUnderstandMyselfAsEvidenceOfCMO,
        [Description("Customer completing a voice of customer survey")]
        CustomerCompletingAVoiceOfCustomerSurvey,
        [Description("Customer interaction via social media")]
        CustomerInteractionViaSocialMedia,
        [Description("Customer IA interaction via telephone")]
        CustomerIAInteractionViaTelephone

    }
}
