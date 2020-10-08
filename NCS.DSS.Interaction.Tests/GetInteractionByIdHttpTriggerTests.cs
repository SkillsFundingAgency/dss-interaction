using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service;
using NCS.DSS.Interaction.Helpers;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
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
        private Mock<ILogger> _log;
        private HttpRequestMessage _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestMessageHelper> _httpRequestMessageHelper;
        private Mock<IGetInteractionByIdHttpTriggerService> _getInteractionByIdHttpTriggerService;
        private Models.Interaction _interaction;
        private GetInteractionByIdHttpTrigger.Function.GetInteractionByIdHttpTrigger function;

        [SetUp]
        public void Setup()
        {
            _interaction = new Models.Interaction();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri = 
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/Interactions/1e1a555c-9633-4e12-ab28-09ed60d51cb")
            };

            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestMessageHelper = new Mock<IHttpRequestMessageHelper>();
            _getInteractionByIdHttpTriggerService = new Mock<IGetInteractionByIdHttpTriggerService>();
            function = new GetInteractionByIdHttpTrigger.Function.GetInteractionByIdHttpTrigger(_resourceHelper.Object, _httpRequestMessageHelper.Object, _getInteractionByIdHttpTriggerService.Object);
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x=>x.GetTouchpointId(_request)).Returns<string>(null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getInteractionByIdHttpTriggerService.Setup(x=>x.GetInteractionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Interaction>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionByIdHttpTrigger_ReturnsStatusCodeOk_WhenInteractionExists()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x=>x.GetTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getInteractionByIdHttpTriggerService.Setup(x=>x.GetInteractionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Interaction>(_interaction));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId)
        {
            return await function.Run(_request, _log.Object, customerId, interactionId).ConfigureAwait(false);
        }
    }
}
