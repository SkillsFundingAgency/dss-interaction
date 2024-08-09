using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;

namespace NCS.DSS.Interaction.DeleteInteractionHttpTrigger
{
    public static class DeleteInteractionHttpTrigger
    {
        [Disable]
        [FunctionName("Delete")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Customers/{customerId}/Interactions/{interactionId}")] HttpRequest req, TraceWriter log, string interactionId)
        {
            log.Info("Delete Interaction C# HTTP trigger function processed a request.");

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