using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace NCS.DSS.Interaction.PostInteractionHttpTrigger
{
    public static class PostInteractionHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Interaction))]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId:guid}/Interactions/")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Post Interaction C# HTTP trigger function processed a request.");

            // Get request body
            var interaction = await req.Content.ReadAsAsync<Models.Interaction>();

            var interactionService = new PostInteractionHttpTriggerService();
            var interactionId = interactionService.Create(interaction);

            return interactionId == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                : new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("Created Interaction record with Id of : " + interactionId)
                };
        }
    }
}