﻿using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.Helpers;
using NCS.DSS.Interaction.PatchInteractionHttpTrigger.Service;
using NCS.DSS.Interaction.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Interaction.Tests
{
    [TestFixture]
    public class PatchInteractionHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private Mock<ILogger<PatchInteractionHttpTrigger.Function.PatchInteractionHttpTrigger>> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private IValidate _validate;
        private Mock<IHttpRequestHelper> _httpRequestMessageHelper;
        private Mock<IPatchInteractionHttpTriggerService> _patchInteractionHttpTriggerService;
        private Models.Interaction _interaction;
        private Models.InteractionPatch _interactionPatch;
        private PatchInteractionHttpTrigger.Function.PatchInteractionHttpTrigger _function;
        private Mock<IDynamicHelper> _dynamicHelper;

        [SetUp]
        public void Setup()
        {
            _interaction = new Models.Interaction();
            _interactionPatch = new Models.InteractionPatch();

            _request = new DefaultHttpContext().Request;
            _dynamicHelper = new Mock<IDynamicHelper>();

            _log = new Mock<ILogger<PatchInteractionHttpTrigger.Function.PatchInteractionHttpTrigger>>();
            _resourceHelper = new Mock<IResourceHelper>();
            _validate = new Validate();
            _httpRequestMessageHelper = new Mock<IHttpRequestHelper>();
            _patchInteractionHttpTriggerService = new Mock<IPatchInteractionHttpTriggerService>();
            _function = new PatchInteractionHttpTrigger.Function.PatchInteractionHttpTrigger(_resourceHelper.Object, _httpRequestMessageHelper.Object, _patchInteractionHttpTriggerService.Object, _validate, _dynamicHelper.Object, _log.Object);
        }

        [Test]
        public async Task PatchInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchInteractionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenInteractionHasFailedValidation()
        {
            // Arrange
            var validate = new Mock<IValidate>();
            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            validate.Setup(x => x.ValidateResource(It.IsAny<Models.InteractionPatch>())).Returns(validationResults);
            _function = new PatchInteractionHttpTrigger.Function.PatchInteractionHttpTrigger(_resourceHelper.Object, _httpRequestMessageHelper.Object, _patchInteractionHttpTriggerService.Object, validate.Object, _dynamicHelper.Object, _log.Object);
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.InteractionPatch>(_request)).Returns(Task.FromResult(_interactionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchInteractionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenInteractionRequestIsInvalid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.InteractionPatch>(_request)).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchInteractionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            //Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.InteractionPatch>(_request)).Returns(Task.FromResult(_interactionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchInteractionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.InteractionPatch>(_request)).Returns(Task.FromResult(_interactionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _patchInteractionHttpTriggerService.Setup(x => x.GetInteractionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Interaction>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateInteractionRecord()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.InteractionPatch>(_request)).Returns(Task.FromResult(_interactionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _patchInteractionHttpTriggerService.Setup(x => x.GetInteractionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Interaction>(_interaction));
            _patchInteractionHttpTriggerService.Setup(x => x.UpdateAsync(It.IsAny<Models.Interaction>(), It.IsAny<Models.InteractionPatch>())).Returns(Task.FromResult<Models.Interaction>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchInteractionHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsNotValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.InteractionPatch>(_request)).Returns(Task.FromResult(_interactionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _patchInteractionHttpTriggerService.Setup(x => x.GetInteractionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Interaction>(_interaction));
            _patchInteractionHttpTriggerService.Setup(x => x.UpdateAsync(It.IsAny<Models.Interaction>(), It.IsAny<Models.InteractionPatch>())).Returns(Task.FromResult<Models.Interaction>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchInteractionHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestMessageHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestMessageHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestMessageHelper.Setup(x => x.GetResourceFromRequest<Models.InteractionPatch>(_request)).Returns(Task.FromResult(_interactionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _patchInteractionHttpTriggerService.Setup(x => x.GetInteractionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Interaction>(_interaction));
            _patchInteractionHttpTriggerService.Setup(x => x.UpdateAsync(It.IsAny<Models.Interaction>(), It.IsAny<Models.InteractionPatch>())).Returns(Task.FromResult<Models.Interaction>(_interaction));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);
            var responseResult = result as JsonResult;
            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId)
        {
            return await _function.Run(_request, customerId, interactionId).ConfigureAwait(false);
        }

    }
}