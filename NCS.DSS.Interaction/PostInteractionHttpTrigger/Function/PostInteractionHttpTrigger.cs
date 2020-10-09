using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.Helpers;
using NCS.DSS.Interaction.PostInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.PostInteractionHttpTrigger.Function
{
    public class PostInteractionHttpTrigger
    {
        IResourceHelper resourceHelper;
        IHttpRequestMessageHelper httpRequestMessageHelper;
        IPostInteractionHttpTriggerService interactionPostService;
        IValidate validate;
        public PostInteractionHttpTrigger(IResourceHelper resourceHelper, IHttpRequestMessageHelper httpRequestMessageHelper, IPostInteractionHttpTriggerService interactionPostService, IValidate validate)
        {
            this.resourceHelper = resourceHelper;
            this.httpRequestMessageHelper = httpRequestMessageHelper;
            this.interactionPostService = interactionPostService;
            this.validate = validate;
        }

        [FunctionName("Post")]
        [ProducesResponseType(typeof(Models.Interaction), (int)HttpStatusCode.Created)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Interaction Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Interaction does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Interaction validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new interaction resource.")]
        public  async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/")] HttpRequestMessage req, ILogger log, string customerId)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            var ApimURL = httpRequestMessageHelper.GetApimURL(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Post Interaction C# HTTP trigger function processed a request. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            Models.Interaction interactionRequest;

            try
            {
                interactionRequest = await httpRequestMessageHelper.GetInteractionFromRequest<Models.Interaction>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (interactionRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            interactionRequest.SetIds(customerGuid, touchpointId);

            var errors = validate.ValidateResource(interactionRequest);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return HttpResponseMessageHelper.Forbidden(customerGuid);

            var interaction = await interactionPostService.CreateAsync(interactionRequest);

            if (interaction != null)
                await interactionPostService.SendToServiceBusQueueAsync(interaction, ApimURL);

            return interaction == null
                ? HttpResponseMessageHelper.BadRequest(customerGuid)
                : HttpResponseMessageHelper.Created(JsonHelper.SerializeObject(interaction));
        }
    }
}