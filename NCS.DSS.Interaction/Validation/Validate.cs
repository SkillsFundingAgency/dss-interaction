﻿using NCS.DSS.Interaction.Models;
using NCS.DSS.Interaction.ReferenceData;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Interaction.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(IInteraction resource)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);
            ValidateInteractionRules(resource, results);

            return results;
        }

        private void ValidateInteractionRules(IInteraction interactionResource, List<ValidationResult> results)
        {
            if (interactionResource == null)
                return;

            if (interactionResource.DateandTimeOfInteraction.HasValue && interactionResource.DateandTimeOfInteraction.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Date and Time Of Interaction must be less the current date/time", new[] { "DateandTimeOfInteraction" }));

            if (interactionResource.LastModifiedDate.HasValue && interactionResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));

            if (interactionResource.Channel.HasValue && !Enum.IsDefined(typeof(Channel), interactionResource.Channel.Value))
                results.Add(new ValidationResult("Please supply a valid Channel", new[] { "Channel" }));

            if (interactionResource.InteractionType.HasValue && !Enum.IsDefined(typeof(InteractionType), interactionResource.InteractionType.Value))
                results.Add(new ValidationResult("Please supply a valid Interaction Type", new[] { "InteractionType" }));

        }

    }
}
