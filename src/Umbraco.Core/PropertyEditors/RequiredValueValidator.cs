using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator that validates that the value is not null or empty (if it is a string)
    /// </summary>
    [ValueValidator("Required")]
    internal sealed class RequiredValueValidator : ManifestValueValidator
    {
        public override IEnumerable<ValidationResult> Validate(object value, string config, string preValues, PropertyEditor editor)
        {
            //TODO: localize these!

            if (value == null)
            {
                yield return new ValidationResult("Value cannot be null");
            }
            if (value is string && ((string)value).IsNullOrWhiteSpace())
            {
                yield return new ValidationResult("Value cannot be empty");
            }
        }
    }
}