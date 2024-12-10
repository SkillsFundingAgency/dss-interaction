using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Interaction.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.Interaction.ServiceBus
{
    public class InteractionServiceBusClient : IInteractionServiceBusClient
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<InteractionServiceBusClient> _logger;
        private readonly string _queueName;

        public InteractionServiceBusClient(ServiceBusClient serviceBusClient,
            IOptions<InteractionConfigurationSettings> configOptions,
            ILogger<InteractionServiceBusClient> logger)
        {
            var config = configOptions.Value;
            if (string.IsNullOrEmpty(config.QueueName))
            {
                throw new ArgumentNullException(nameof(config.QueueName), "QueueName cannot be null or empty.");
            }

            _serviceBusClient = serviceBusClient;
            _queueName = config.QueueName;
            _logger = logger;
        }

        public async Task SendPostMessageAsync(Models.Interaction interaction, string reqUrl)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(_queueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Interaction record {" + interaction.InteractionId + "} added at " + DateTime.UtcNow,
                CustomerGuid = interaction.CustomerId,
                LastModifiedDate = interaction.LastModifiedDate,
                URL = reqUrl + "/" + interaction.InteractionId,
                IsNewCustomer = false,
                TouchpointId = interaction.LastModifiedTouchpointId
            };

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = interaction.CustomerId + " " + DateTime.UtcNow
            };

            _logger.LogInformation("Attempting to send POST message to service bus. Interaction ID: {InteractionId}", interaction.InteractionId);

            await serviceBusSender.SendMessageAsync(msg);

            _logger.LogInformation("Successfully sent POST message to the service bus. Interaction ID: {InteractionId}", interaction.InteractionId);
        }

        public async Task SendPatchMessageAsync(Models.Interaction interaction, Guid customerId, string reqUrl)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(_queueName);
            var messageModel = new MessageModel
            {
                TitleMessage = "Interaction record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = interaction.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = interaction.LastModifiedTouchpointId
            };
            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            _logger.LogInformation("Attempting to send PATCH message to service bus. Interaction ID: {InteractionId}", interaction.InteractionId);

            await serviceBusSender.SendMessageAsync(msg);

            _logger.LogInformation("Successfully sent PATCH message to the service bus. Interaction ID: {InteractionId}", interaction.InteractionId);
        }
    }
}

