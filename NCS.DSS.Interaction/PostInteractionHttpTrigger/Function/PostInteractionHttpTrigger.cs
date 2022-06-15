using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Helper;
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
        private IResourceHelper _resourceHelper;
        private IHttpRequestHelper _httpRequestMessageHelper;
        private IPostInteractionHttpTriggerService _interactionPostService;
        private IValidate _validate;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        public PostInteractionHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper httpRequestMessageHelper, IPostInteractionHttpTriggerService interactionPostService, IValidate validate, IHttpResponseMessageHelper httpResponseMessageHelper, IJsonHelper jsonHelper)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _interactionPostService = interactionPostService;
            _validate = validate;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
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
        public  async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/")] HttpRequest req, ILogger log, string customerId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return _httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = _httpRequestMessageHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                log.LogInformation("Unable to locate 'SubcontractorId' in request header.");
                return _httpResponseMessageHelper.BadRequest();
            }

            var ApimURL = _httpRequestMessageHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Post Interaction C# HTTP trigger function processed a request. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);

            Models.Interaction interactionRequest;

            try
            {
                interactionRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.Interaction>(req);
            }
            catch (JsonException ex)
            {
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (interactionRequest == null)
                return _httpResponseMessageHelper.UnprocessableEntity(req);

            interactionRequest.SetIds(customerGuid, touchpointId);

            var errors = _validate.ValidateResource(interactionRequest);

            if (errors != null && errors.Any())
                return _httpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return _httpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return _httpResponseMessageHelper.Forbidden(customerGuid);

            var interaction = await _interactionPostService.CreateAsync(interactionRequest);

            if (interaction != null)
                await _interactionPostService.SendToServiceBusQueueAsync(interaction, ApimURL);

            return interaction == null
                ? _httpResponseMessageHelper.BadRequest(customerGuid)
                : _httpResponseMessageHelper.Created(_jsonHelper.SerializeObjectAndRenameIdProperty(interaction, "id", "InteractionId"));
        }
    }
}