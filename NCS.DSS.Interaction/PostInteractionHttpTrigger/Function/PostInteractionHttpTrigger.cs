using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.Helpers;
using NCS.DSS.Interaction.PostInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Interaction.PostInteractionHttpTrigger.Function
{
    public class PostInteractionHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IPostInteractionHttpTriggerService _interactionPostService;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger<PostInteractionHttpTrigger> _logger;

        public PostInteractionHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper httpRequestMessageHelper, IPostInteractionHttpTriggerService interactionPostService, IValidate validate, IDynamicHelper dynamicHelper, ILogger<PostInteractionHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _interactionPostService = interactionPostService;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.Interaction), (int)HttpStatusCode.Created)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Interaction Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Interaction does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Interaction validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new interaction resource.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/")] HttpRequest req, string customerId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PostInteractionHttpTrigger));

            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header.");
            }

            var ApimURL = _httpRequestMessageHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                _logger.LogInformation("Unable to locate 'apimurl' in request header");
                return new BadRequestObjectResult("Unable to locate 'apimurl' in request header");
            }

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}.", touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult($"Unable to parse 'customerId' to a GUID. Customer GUID: {customerId}");
            }

            _logger.LogInformation("Attempting to retrieve resource from request.");
            Models.Interaction interactionRequest;
            try
            {
                interactionRequest = await _httpRequestMessageHelper.GetResourceFromRequest<Models.Interaction>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to parse {interactionRequest} from request body. Exception: {ExceptionMessage}", nameof(interactionRequest), ex.Message);
                return new UnprocessableEntityObjectResult($"Unable to extract interaction data from request. Exception is: {ex.Message}");
            }

            if (interactionRequest == null)
            {
                _logger.LogWarning("{interactionRequest} object is NULL.", nameof(interactionRequest));
                return new UnprocessableEntityObjectResult($"Please ensure data has been added to the request body. Resource returned NULL when extracted from request for customer {customerId}.");
            }

            interactionRequest.SetIds(customerGuid, touchpointId);

            _logger.LogInformation("Attempting to validate {interactionRequest} object", nameof(interactionRequest));
            var errors = _validate.ValidateResource(interactionRequest);
            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Falied to validate {interactionRequest} object", nameof(interactionRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {interactionRequest} object", nameof(interactionRequest));

            _logger.LogInformation("Input validation has succeeded. Touchpoint ID: {TouchpointId}.", touchpointId);

            _logger.LogInformation("Checking if customer exists. Customer ID: {CustomerId}.", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer ID: {CustomerId}.", customerGuid);
                return new NotFoundObjectResult($"Customer does not exist. Customer ID: {customerGuid}");
            }

            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);

            _logger.LogInformation("Check if customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);
            if (isCustomerReadOnly)
            {
                _logger.LogWarning("Customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
                return new ObjectResult($"Customer is read-only. Customer GUID: {customerGuid}")
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            _logger.LogInformation("Attempting to POST Interaction in Cosmos DB. Interaction GUID: {InteractionGuid}", interactionRequest.InteractionId);
            var interaction = await _interactionPostService.CreateAsync(interactionRequest);

            if (interaction != null)
            {
                _logger.LogInformation("Successfully POSTed Interaction in Cosmos DB. Interaction GUID: {InteractionGuid}", interaction.InteractionId);
                _logger.LogInformation("Attempting to send message to Service Bus Namespace. Interaction GUID: {InteractionGuid}", interaction.InteractionId);
                await _interactionPostService.SendToServiceBusQueueAsync(interaction, ApimURL);
                _logger.LogInformation("Successfully sent message to Service Bus. Interaction GUID: {InteractionGuid}", interaction.InteractionId);
            }

            if (interaction == null)
            {
                _logger.LogWarning("POST request unsuccessful. Interaction GUID: {InteractionGuid}", interactionRequest.InteractionId);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostInteractionHttpTrigger));
                return new BadRequestObjectResult($"Failed to create interaction for customer ID: {customerGuid}.");
            }

            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostInteractionHttpTrigger));

            return new JsonResult(interaction, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}