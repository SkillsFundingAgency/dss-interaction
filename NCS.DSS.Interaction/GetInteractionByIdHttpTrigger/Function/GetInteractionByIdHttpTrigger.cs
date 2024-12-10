using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Function
{
    public class GetInteractionByIdHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetInteractionByIdHttpTriggerService _interactionGetService;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly ILogger<GetInteractionByIdHttpTrigger> _logger;

        public GetInteractionByIdHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper httpRequestMessageHelper, IGetInteractionByIdHttpTriggerService interactionGetService, ILogger<GetInteractionByIdHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _interactionGetService = interactionGetService;
            _logger = logger;
        }


        [Function("GetById")]
        [ProducesResponseTypeAttribute(typeof(Models.Interaction), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Interaction found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Interaction does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual interaction record.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}")] HttpRequest req, string customerId, string interactionId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetInteractionByIdHttpTrigger));

            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

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

            _logger.LogInformation("Checking if customer exists. Customer ID: {CustomerId}.", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer not found. Customer ID: {CustomerId}.", customerGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);

            _logger.LogInformation("Retrieving interaction for Customer ID: {CustomerId}, Interaction ID: {InteractionId}.", customerGuid, interactionGuid);
            var interaction = await _interactionGetService.GetInteractionForCustomerAsync(customerGuid, interactionGuid);

            if (interaction == null)
            {
                _logger.LogWarning("Interaction not found for Customer ID: {CustomerId}, Interaction ID: {InteractionId}.", customerGuid, interactionGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetInteractionByIdHttpTrigger));
                return new NoContentResult();
            }

            _logger.LogInformation("Interaction successfully retrieved for Customer With ID {CustomerGuid}. Interaction GUID: {InteractionGuid}", customerGuid, interactionGuid);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetInteractionByIdHttpTrigger));
            return new JsonResult(interaction, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}