using NCS.DSS.Interaction.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Interaction.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IInteraction resource);
    }
}