using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.Tests
{
    [TestFixture]
    public class GetInteractionHttpTriggerTest
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private Mock<ILogger> _log;
        private HttpRequestMessage _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestMessageHelper> _httpRequestMessageHelper;
        private Mock<IGetInteractionHttpTriggerService> _getInteractionHttpTriggerService;
        private GetInteractionHttpTrigger.Function.GetInteractionHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri =
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/Interactions/")
            };

            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper >();
            _httpRequestMessageHelper = new Mock<IHttpRequestMessageHelper>();
            _getInteractionHttpTriggerService = new Mock<IGetInteractionHttpTriggerService>();

            _function = new GetInteractionHttpTrigger.Function.GetInteractionHttpTrigger(_resourceHelper.Object, _httpRequestMessageHelper.Object, _getInteractionHttpTriggerService.Object);
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x=>x.GetTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            _httpRequestMessageHelper.Setup(x=>x.GetTouchpointId(_request)).Returns("0000000001");
            var result = await RunFunction(InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getInteractionHttpTriggerService.Setup(x=>x.GetInteractionsAsync(It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.Interaction>>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeOk_WhenInteractionExists()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            var listOfInteractiones = new List<Models.Interaction>();
            _getInteractionHttpTriggerService.Setup(x=>x.GetInteractionsAsync(It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.Interaction>>(listOfInteractiones));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await _function.Run(_request, _log.Object, customerId).ConfigureAwait(false);
        }

    }
}