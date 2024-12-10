using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.Helpers;
using NCS.DSS.Interaction.Models;
using NCS.DSS.Interaction.PatchInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Interaction.PatchInteractionHttpTrigger.Function
{
    public class PatchInteractionHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IPatchInteractionHttpTriggerService _interactionPatchService;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger<PatchInteractionHttpTrigger> _logger;

        public PatchInteractionHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper httpRequestMessageHelper, IPatchInteractionHttpTriggerService interactionPatchService, IValidate validate, IDynamicHelper dynamicHelper, ILogger<PatchInteractionHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _interactionPatchService = interactionPatchService;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
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
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PatchInteractionHttpTrigger));

            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var ApimURL = _httpRequestMessageHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _logger.LogInformation("Unable to locate 'apimurl' in request header");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Patch Interaction C# HTTP trigger function processed a request. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult(customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _logger.LogWarning("Unable to parse 'interactionId' to a GUID. Interaction ID: {InteractionId}", interactionId);
                return new BadRequestObjectResult(interactionGuid);
            }

            _logger.LogInformation("Attempting to retrieve resource from request.");
            InteractionPatch interactionPatchRequest;
            try
            {
                interactionPatchRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.InteractionPatch>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to parse {interactionPatchRequest} from request body. Exception: {ExceptionMessage}", nameof(interactionPatchRequest), ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (interactionPatchRequest == null)
            {
                _logger.LogWarning("{interactionPatchRequest} object is NULL.", nameof(interactionPatchRequest));
                return new UnprocessableEntityObjectResult(req);
            }

            interactionPatchRequest.LastModifiedTouchpointId = touchpointId;

            _logger.LogInformation("Attempting to validate {interactionPatchRequest} object", nameof(interactionPatchRequest));
            var errors = _validate.ValidateResource(interactionPatchRequest);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Falied to validate {outcomeValidationObject} object", nameof(interactionPatchRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {outcomeValidationObject} object", nameof(interactionPatchRequest));


            _logger.LogInformation("Checking if customer exists. Customer ID: {CustomerId}.", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer not found. Customer ID: {CustomerId}.", customerGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);

            _logger.LogInformation("Check if customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("Customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            _logger.LogInformation("Retrieving interaction for Customer ID: {CustomerId}, Interaction ID: {InteractionId}.", customerGuid, interactionGuid);
            var interaction = await _interactionPatchService.GetInteractionForCustomerAsync(customerGuid, interactionGuid);

            if (interaction == null)
            {
                _logger.LogWarning("Interaction not found for Customer ID: {CustomerId}, Interaction ID: {InteractionId}.", customerGuid, interactionGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Interaction successfully retrieved for Customer With ID {CustomerGuid}. Interaction GUID: {InteractionGuid}", customerGuid, interactionGuid);

            _logger.LogInformation("Attempting to PATCH Interaction in Cosmos DB. Interaction GUID: {InteractionGuid}", interactionGuid);
            var updatedInteraction = await _interactionPatchService.UpdateAsync(interaction, interactionPatchRequest);

            if (updatedInteraction != null)
            {
                _logger.LogInformation("Successfully PATCHed Interaction in Cosmos DB. Interaction GUID: {InteractionGuid}", interactionGuid);
                _logger.LogInformation("Attempting to send message to Service Bus Namespace. Interaction GUID: {InteractionGuid}", interactionGuid);
                await _interactionPatchService.SendToServiceBusQueueAsync(updatedInteraction, customerGuid, ApimURL);
                _logger.LogInformation("Successfully sent message to Service Bus. Interaction GUID: {InteractionGuid}", interactionGuid);

            }

            if (updatedInteraction == null)
            {
                _logger.LogWarning("PATCH request unsuccessful. Interaction GUID: {InteractionGuid}", interactionGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchInteractionHttpTrigger));
                return new BadRequestObjectResult(interactionGuid);
            }

            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchInteractionHttpTrigger));
            return new JsonResult(updatedInteraction, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}