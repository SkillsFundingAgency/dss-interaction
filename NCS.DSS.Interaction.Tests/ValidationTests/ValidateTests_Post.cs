using NCS.DSS.Interaction.Validation;
using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Interaction.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests_Post
    {
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _validate = new Validate();
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsValid()
        {
            var interaction = new Models.Interaction
            {
                Channel = ReferenceData.Channel.Telephone,
                InteractionType = ReferenceData.InteractionType.WebChat,
                LastModifiedTouchpointId = "0000000001"
            };

            var result = _validate.ValidateResource(interaction);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count.Equals(0));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsInvalid()
        {
            var interaction = new Models.Interaction
            {
                Channel = ReferenceData.Channel.Telephone, 
                InteractionType = ReferenceData.InteractionType.WebChat, 
                LastModifiedTouchpointId = "000000000A"
            };

            var result = _validate.ValidateResource(interaction);

            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count.Equals(1));
        }
    }
}