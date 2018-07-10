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
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service;
using NCS.DSS.Interaction.Helpers;
using NCS.DSS.Interaction.Ioc;

namespace NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Function
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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetInteractionByIdHttpTriggerService interactionGetService)
        {
            log.Info("Get Interaction By Id C# HTTP trigger function  processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var interaction = await interactionGetService.GetInteractionForCustomerAsync(customerGuid, interactionGuid);

            return interaction == null ? 
                HttpResponseMessageHelper.NoContent(interactionGuid) :
                HttpResponseMessageHelper.Ok(interaction);
        }
    }
}