using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.Models;
using NCS.DSS.Interaction.PatchInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using NCS.DSS.Interaction.Helpers;
using System.Text.Json;

namespace NCS.DSS.Interaction.PatchInteractionHttpTrigger.Function
{
    public class PatchInteractionHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IHttpRequestHelper _httpRequestMessageHelper;
        private IPatchInteractionHttpTriggerService _interactionPatchService;
        private IValidate _validate;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private IDynamicHelper _dynamicHelper;
        private ILogger log;

        public PatchInteractionHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper httpRequestMessageHelper, IPatchInteractionHttpTriggerService interactionPatchService, IValidate validate, IHttpResponseMessageHelper httpResponseMessageHelper, IJsonHelper jsonHelper, IDynamicHelper dynamicHelper, ILogger<PatchInteractionHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _interactionPatchService = interactionPatchService;
            _validate = validate;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _dynamicHelper = dynamicHelper;
            log = logger;
        }

        [Function("Patch")]
        [ProducesResponseTypeAttribute(typeof(Models.Interaction), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Interaction Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Interaction does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Interaction validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an interaction record.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}")] HttpRequest req, string customerId, string interactionId)
        {
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var ApimURL = _httpRequestMessageHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            log.LogInformation("Patch Interaction C# HTTP trigger function processed a request. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return new BadRequestObjectResult(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return new BadRequestObjectResult(interactionGuid);

            InteractionPatch interactionPatchRequest;

            try
            {
                interactionPatchRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.InteractionPatch>(req);
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (interactionPatchRequest == null)
                return new UnprocessableEntityObjectResult(req);

            interactionPatchRequest.LastModifiedTouchpointId = touchpointId;

            var errors = _validate.ValidateResource(interactionPatchRequest);

            if (errors != null && errors.Any())
                return new UnprocessableEntityObjectResult(errors);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return new NoContentResult();

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };

            var interaction = await _interactionPatchService.GetInteractionForCustomerAsync(customerGuid, interactionGuid);

            if (interaction == null)
                return new NoContentResult();

            var updatedInteraction = await _interactionPatchService.UpdateAsync(interaction, interactionPatchRequest);

            if (updatedInteraction != null)
                await _interactionPatchService.SendToServiceBusQueueAsync(updatedInteraction, customerGuid, ApimURL);

            return updatedInteraction == null ?
                new BadRequestObjectResult(interactionGuid) :
                new JsonResult(updatedInteraction, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
        }
    }
}