using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.GetInteractionHttpTrigger
{
    public static class GetInteractionHttpTrigger
    {
        [FunctionName("Get")]
        [Display(Name = "Put", Description = "Ability to return all interactions for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/")]HttpRequestMessage req, TraceWriter log, string customerId)
        {
            log.Info("Get Interactions C# HTTP trigger function processed a request.");

            var service = new GetInteractionHttpTriggerService();
            var values = await service.GetInteractions();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}