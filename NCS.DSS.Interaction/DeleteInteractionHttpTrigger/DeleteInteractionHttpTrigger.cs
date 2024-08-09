using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace NCS.DSS.Interaction.DeleteInteractionHttpTrigger
{
    public static class DeleteInteractionHttpTrigger
    {
        [Disable]
        [Function("Delete")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Customers/{customerId}/Interactions/{interactionId}")] HttpRequest req, ILogger log, string interactionId)
        {
            log.LogInformation("Delete Interaction C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(interactionId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Deleted Interaction record with Id of : " + interactionGuid)
            };
        }
    }
}