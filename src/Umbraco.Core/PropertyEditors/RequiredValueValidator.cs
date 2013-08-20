using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator that validates that the value is not null or empty (if it is a string)
    /// </summary>
    [ValueValidator("Required")]
    internal sealed class RequiredValueValidator : ValueValidator
    {
        public override IEnumerable<ValidationResult> Validate(string value, string config, string preValues, PropertyEditor editor)
        {
            //TODO: localize these!

            if (value == null)
            {
                yield return new ValidationResult("Value cannot be null", new[] { "value" });
            }
            if (value.IsNullOrWhiteSpace())
            {
                yield return new ValidationResult("Value cannot be empty", new[] { "value" });
            }
        }
    }
}