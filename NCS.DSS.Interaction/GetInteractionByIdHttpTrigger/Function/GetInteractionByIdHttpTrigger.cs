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
        private IResourceHelper _resourceHelper;
        private IGetInteractionByIdHttpTriggerService _interactionGetService;
        private IHttpRequestHelper _httpRequestMessageHelper;
        private ILogger log;

        public GetInteractionByIdHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper httpRequestMessageHelper, IGetInteractionByIdHttpTriggerService interactionGetService, ILogger<GetInteractionByIdHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _interactionGetService = interactionGetService;
            log = logger;
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
            var touchpointId = _httpRequestMessageHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            log.LogInformation("Get Interaction By Id C# HTTP trigger function  processed a request. By Touchpoint. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return new BadRequestObjectResult(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return new BadRequestObjectResult(interactionGuid);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return new NoContentResult();

            var interaction = await _interactionGetService.GetInteractionForCustomerAsync(customerGuid, interactionGuid);

            return interaction == null ?
                new NoContentResult() :
                new JsonResult(interaction, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
        }
    }
}