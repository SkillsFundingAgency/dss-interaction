using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.Tests
{
    [TestFixture]
    public class GetInteractionByIdHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private readonly Guid _interactionId = Guid.Parse("aa57e39e-4469-4c79-a9e9-9cb4ef410382");
        private Mock<ILogger<GetInteractionByIdHttpTrigger.Function.GetInteractionByIdHttpTrigger>> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<IGetInteractionByIdHttpTriggerService> _getInteractionByIdHttpTriggerService;
        private Models.Interaction _interaction;
        private GetInteractionByIdHttpTrigger.Function.GetInteractionByIdHttpTrigger function;

        [SetUp]
        public void Setup()
        {
            _interaction = new Models.Interaction();

            _request = new DefaultHttpContext().Request;

            _log = new Mock<ILogger<GetInteractionByIdHttpTrigger.Function.GetInteractionByIdHttpTrigger>>();
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _getInteractionByIdHttpTriggerService = new Mock<IGetInteractionByIdHttpTriggerService>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            function = new GetInteractionByIdHttpTrigger.Function.GetInteractionByIdHttpTrigger(_resourceHelper.Object, _httpRequestMessageHelper.Object, _getInteractionByIdHttpTriggerService.Object, _log.Object);
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns<string>(null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getInteractionByIdHttpTriggerService.Setup(x => x.GetInteractionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Interaction>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeOk_WhenInteractionExists()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getInteractionByIdHttpTriggerService.Setup(x => x.GetInteractionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Interaction>(_interaction));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);
            var responseResult = result as JsonResult;
            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId)
        {
            return await function.Run(_request, customerId, interactionId).ConfigureAwait(false);
        }
    }
}
