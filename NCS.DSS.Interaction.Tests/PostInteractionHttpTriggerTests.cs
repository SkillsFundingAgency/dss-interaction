using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private Mock<ILogger<PostInteractionHttpTrigger.Function.PostInteractionHttpTrigger>> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IValidate> _validate;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<IPostInteractionHttpTriggerService> _postInteractionHttpTriggerService;
        private Models.Interaction _interaction;
        private PostInteractionHttpTrigger.Function.PostInteractionHttpTrigger _function;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Mock<IDynamicHelper> _dynamicHelper;

        [SetUp]
        public void Setup()
        {
            _interaction = new Models.Interaction();

            _request = new DefaultHttpContext().Request;
            _log = new Mock<ILogger<PostInteractionHttpTrigger.Function.PostInteractionHttpTrigger>>();
            _dynamicHelper = new Mock<IDynamicHelper>();
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _validate = new Mock<IValidate>();
            _postInteractionHttpTriggerService = new Mock<IPostInteractionHttpTriggerService>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();
            _function = new PostInteractionHttpTrigger.Function.PostInteractionHttpTrigger(_resourceHelper.Object, _httpRequestMessageHelper.Object, _postInteractionHttpTriggerService.Object, _validate.Object, _httpResponseMessageHelper, _jsonHelper, _dynamicHelper.Object, _log.Object);
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x=>x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");

            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenInteractionHasFailedValidation()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetResourceFromRequest<Models.Interaction>(_request)).Returns(Task.FromResult(_interaction));

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.Setup(x=>x.ValidateResource(It.IsAny<Models.Interaction>())).Returns(validationResults);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenInteractionRequestIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetResourceFromRequest<Models.InteractionPatch>(_request)).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetResourceFromRequest<Models.Interaction>(_request)).Returns(Task.FromResult(_interaction));
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateInteractionRecord()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetResourceFromRequest<Models.Interaction>(_request)).Returns(Task.FromResult(_interaction));
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postInteractionHttpTriggerService.Setup(x=>x.CreateAsync(It.IsAny<Models.Interaction>())).Returns(Task.FromResult<Models.Interaction>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsNotValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetResourceFromRequest<Models.Interaction>(_request)).Returns(Task.FromResult(_interaction));
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postInteractionHttpTriggerService.Setup(x=>x.CreateAsync(It.IsAny<Models.Interaction>())).Returns(Task.FromResult<Models.Interaction>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostInteractionHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x=>x.GetResourceFromRequest<Models.Interaction>(_request)).Returns(Task.FromResult(_interaction));
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postInteractionHttpTriggerService.Setup(x=>x.CreateAsync(It.IsAny<Models.Interaction>())).Returns(Task.FromResult<Models.Interaction>(_interaction));

            // Act
            var result = await RunFunction(ValidCustomerId);
            var responseResult = result as JsonResult;
            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }

        private async Task<IActionResult> RunFunction(string customerId)
        {
            return await _function.Run(_request, customerId).ConfigureAwait(false);
        }

    }
}