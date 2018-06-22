using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Interaction.Annotations;
using Newtonsoft.Json;

namespace NCS.DSS.Interaction.PatchInteractionHttpTrigger
{
    public static class PatchInteractionHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.Interaction))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Interaction Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Interaction does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Interaction validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an interaction record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId)
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