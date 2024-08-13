using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.Interaction.ServiceBus
{
    public static class ServiceBusClient
    {
        public static readonly string KeyName = Environment.GetEnvironmentVariable("KeyName");
        public static readonly string AccessKey = Environment.GetEnvironmentVariable("AccessKey");
        public static readonly string BaseAddress = Environment.GetEnvironmentVariable("BaseAddress");
        public static readonly string QueueName = Environment.GetEnvironmentVariable("QueueName");
        public static readonly string ServiceBusConnectionString = $"Endpoint={BaseAddress};SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={AccessKey}";
        public static async Task SendPostMessageAsync(Models.Interaction interaction, string reqUrl)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Interaction record {" + interaction.InteractionId + "} added at " + DateTime.UtcNow,
                CustomerGuid = interaction.CustomerId,
                LastModifiedDate = interaction.LastModifiedDate,
                URL = reqUrl + "/" + interaction.InteractionId,
                IsNewCustomer = false,
                TouchpointId = interaction.LastModifiedTouchpointId
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = interaction.CustomerId + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }

        public static async Task SendPatchMessageAsync(Models.Interaction interaction, Guid customerId, string reqUrl)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            var messageModel = new MessageModel
            {
                TitleMessage = "Interaction record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = interaction.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = interaction.LastModifiedTouchpointId
            };
            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };
            await queueClient.SendAsync(msg);
        }

    }

    public class MessageModel
    {
        public string TitleMessage { get; set; }
        public Guid? CustomerGuid { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string URL { get; set; }
        public bool IsNewCustomer { get; set; }
        public string TouchpointId { get; set; }
    }

}

