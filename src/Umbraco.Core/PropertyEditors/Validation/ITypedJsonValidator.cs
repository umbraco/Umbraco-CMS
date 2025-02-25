using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

public interface ITypedJsonValidator<TValue, TConfiguration>
{
    public abstract IEnumerable<ValidationResult> Validate(
        TValue? value,
        TConfiguration? configuration,
        string? valueType,
        PropertyValidationContext validationContext);
}
