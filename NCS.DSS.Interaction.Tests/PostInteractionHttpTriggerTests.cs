using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.Helpers;
using NCS.DSS.Interaction.PostInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.Tests
{
    [TestFixture]
    public class PostInteractionHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private Mock<ILogger> _log;
        private HttpRequestMessage _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IValidate> _validate;
        private Mock<IHttpRequestMessageHelper> _httpRequestMessageHelper;
        private Mock<IPostInteractionHttpTriggerService> _postInteractionHttpTriggerService;
        private Models.Interaction _interaction;
        private PostInteractionHttpTrigger.Function.PostInteractionHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _interaction = new Models.Interaction();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri =
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/Interactions/")
            };
            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestMessageHelper = new Mock<IHttpRequestMessageHelper>();
            _validate = new Mock<IValidate>();
            _postInteractionHttpTriggerService = new Mock<IPostInteractionHttpTriggerService>();
            _function = new PostInteractionHttpTrigger.Function.PostInteractionHttpTrigger(_resourceHelper.Object, _httpRequestMessageHelper.Object, _postInteractionHttpTriggerService.Object, _validate.Object);
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
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
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x=>x.GetTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x=>x.GetApimURL(_request)).Returns("http://localhost:7071/");

            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenInteractionHasFailedValidation()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetApimURL(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetInteractionFromRequest<Models.Interaction>(_request)).Returns(Task.FromResult(_interaction));

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.Setup(x=>x.ValidateResource(It.IsAny<Models.Interaction>())).Returns(validationResults);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenInteractionRequestIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetApimURL(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetInteractionFromRequest<Models.InteractionPatch>(_request)).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetApimURL(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetInteractionFromRequest<Models.Interaction>(_request)).Returns(Task.FromResult(_interaction));
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateInteractionRecord()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetApimURL(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetInteractionFromRequest<Models.Interaction>(_request)).Returns(Task.FromResult(_interaction));
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postInteractionHttpTriggerService.Setup(x=>x.CreateAsync(It.IsAny<Models.Interaction>())).Returns(Task.FromResult<Models.Interaction>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsNotValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetApimURL(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetInteractionFromRequest<Models.Interaction>(_request)).Returns(Task.FromResult(_interaction));
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postInteractionHttpTriggerService.Setup(x=>x.CreateAsync(It.IsAny<Models.Interaction>())).Returns(Task.FromResult<Models.Interaction>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetApimURL(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetInteractionFromRequest<Models.Interaction>(_request)).Returns(Task.FromResult(_interaction));
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postInteractionHttpTriggerService.Setup(x=>x.CreateAsync(It.IsAny<Models.Interaction>())).Returns(Task.FromResult<Models.Interaction>(_interaction));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await _function.Run(_request, _log.Object, customerId).ConfigureAwait(false);
        }

    }
}