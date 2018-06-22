using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Interaction.Annotations;

namespace NCS.DSS.Interaction.PostInteractionHttpTrigger
{
    public static class PostInteractionHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Interaction))]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Interaction Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Interaction does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Interaction validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new interaction resource.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/")]HttpRequestMessage req, TraceWriter log, string customerId)
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