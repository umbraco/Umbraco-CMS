using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Core.PropertyEditors.Validators
{
    /// <summary>
    /// A validator that validates that the value is a valid integer
    /// </summary>
    [ValueValidator("Integer")]
    internal sealed class IntegerValidator : ManifestValueValidator, IPropertyValidator
    {
        public override IEnumerable<ValidationResult> Validate(object value, string validatorConfiguration, object dataTypeConfiguration, PropertyEditor editor)
        {
            if (value != null && value.ToString() != string.Empty)
            {
                var result = value.TryConvertTo<int>();
                if (result.Success == false)
                {
                    yield return new ValidationResult("The value " + value + " is not a valid integer", new[] { "value" });
                }
            }
        }

        public IEnumerable<ValidationResult> Validate(object value, object dataTypeConfiguration, PropertyEditor editor)
        {
            return Validate(value, "", dataTypeConfiguration, editor);
        }
    }
}
