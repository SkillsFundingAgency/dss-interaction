using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Interaction.GetInteractionHttpTrigger.Function
{
    public class GetInteractionHttpTrigger
    {

        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IGetInteractionHttpTriggerService _interactionGetService;
        private readonly ILogger<GetInteractionHttpTrigger> _logger;

        public GetInteractionHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper httpRequestMessageHelper, IGetInteractionHttpTriggerService interactionGetService, ILogger<GetInteractionHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _interactionGetService = interactionGetService;
            _logger = logger;
        }

        [Function("Get")]
        [ProducesResponseType(typeof(Models.Interaction), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Interactions found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Interactions do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Put", Description = "Ability to return all interactions for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/")] HttpRequest req, string customerId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetInteractionHttpTrigger));

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

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}", touchpointId);

            _logger.LogInformation("Checking if customer exists. Customer ID: {CustomerId}.", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer not found. Customer ID: {CustomerId}.", customerGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);

            _logger.LogInformation("Retrieving interactions for Customer ID: {CustomerId}.", customerGuid);
            var interactions = await _interactionGetService.GetInteractionsAsync(customerGuid);

            if (interactions == null || interactions.Count == 0)
            {
                _logger.LogWarning("No interactions found for Customer ID: {CustomerId}.", customerGuid);
                return new NoContentResult();
            }

            if (interactions.Count == 1)
            {
                _logger.LogInformation("Single interaction found for Customer ID: {CustomerId}.", customerGuid);
                return new JsonResult(interactions[0], new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            _logger.LogInformation("Multiple interactions retrieved for Customer ID: {CustomerId}. Count: {Count}.", customerGuid, interactions.Count);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetInteractionHttpTrigger));

            return new JsonResult(interactions, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };

        }
    }
}