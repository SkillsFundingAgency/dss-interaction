namespace NCS.DSS.Interaction.Models
{
    public class InteractionConfigurationSettings
    {
        public required string AccessKey { get; set; }
        public required string BaseAddress { get; set; }
        public required string CollectionId { get; set; }
        public required string CustomerCollectionId { get; set; }
        public required string CustomerDatabaseId { get; set; }
        public required string DatabaseId { get; set; }
        public required string Endpoint { get; set; }
        public required string Key { get; set; }
        public required string KeyName { get; set; }
        public required string QueueName { get; set; }
    }
}