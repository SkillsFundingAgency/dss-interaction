using NCS.DSS.Interaction.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Interaction.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IInteraction resource);
    }
}