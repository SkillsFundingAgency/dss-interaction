﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Interaction.Cosmos.Helper;
using NCS.DSS.Interaction.GetInteractionByIdHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Interaction.Tests
{
    [TestFixture]
    public class GetInteractionByIdHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private readonly Guid _interactionId = Guid.Parse("aa57e39e-4469-4c79-a9e9-9cb4ef410382");
        private TraceWriter _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IGetInteractionByIdHttpTriggerService _getInteractionByIdHttpTriggerService;
        private Models.Interaction _interaction;

        [SetUp]
        public void Setup()
        {
            _interaction = Substitute.For<Models.Interaction>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri = 
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/Interactions/1e1a555c-9633-4e12-ab28-09ed60d51cb")
            };
            _log = new TraceMonitor();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _getInteractionByIdHttpTriggerService = Substitute.For<IGetInteractionByIdHttpTriggerService>();
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            _getInteractionByIdHttpTriggerService.GetInteractionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Interaction>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetInteractionHttpTrigger_ReturnsStatusCodeOk_WhenInteractionExists()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            _getInteractionByIdHttpTriggerService.GetInteractionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Interaction>(_interaction).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId)
        {
            return await GetInteractionByIdHttpTrigger.Function.GetInteractionByIdHttpTrigger.Run(
                _request, _log, customerId, interactionId, _resourceHelper, _getInteractionByIdHttpTriggerService).ConfigureAwait(false);
        }

    }
}