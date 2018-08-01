using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Interaction.Models;

namespace NCS.DSS.Interaction.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IInteraction resource);
    }
}