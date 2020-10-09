using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionHttpTrigger.Service;
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
        private DefaultHttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<IGetInteractionHttpTriggerService> _getInteractionHttpTriggerService;
        private GetInteractionHttpTrigger.Function.GetInteractionHttpTrigger _function;
        private GetInteractionByIdHttpTrigger.Function.GetInteractionByIdHttpTrigger function;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        [SetUp]
        public void Setup()
        {
            _request = null;

            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper >();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _getInteractionHttpTriggerService = new Mock<IGetInteractionHttpTriggerService>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();

            _function = new GetInteractionHttpTrigger.Function.GetInteractionHttpTrigger(_resourceHelper.Object, _httpRequestMessageHelper.Object, _getInteractionHttpTriggerService.Object, _httpResponseMessageHelper, _jsonHelper);
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns((string)null);

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
            _httpRequestMessageHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns("0000000001");
            var result = await RunFunction(InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
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
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
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
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
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