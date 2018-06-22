using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using NCS.DSS.Interaction.Annotations;

namespace NCS.DSS.Interaction.GetInteractionByIdHttpTrigger
{
    public static class GetInteractionByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ResponseType(typeof(Models.Interaction))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Interaction found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Interaction does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual interaction record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId)
        {
            log.Info("Get Interaction By Id C# HTTP trigger function  processed a request.");

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(interactionId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var service = new GetInteractionByIdHttpTriggerService();
            var values = await service.GetInteraction(interactionGuid);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}