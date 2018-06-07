using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.Interaction.PatchInteractionHttpTrigger
{
    public static class PatchInteractionHttpTrigger
    {
        [FunctionName("Patch")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}")]HttpRequestMessage req, TraceWriter log, string interactionId)
        {
            log.Info("Patch Interaction C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Updated Interaction record with Id of : " + interactionGuid)
            };
        }
    }
}