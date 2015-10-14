using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator that validates that the value is a valid decimal
    /// </summary>
    [ValueValidator("Decimal")]
    internal sealed class DecimalValidator : ManifestValueValidator, IPropertyValidator
    {
        public override IEnumerable<ValidationResult> Validate(object value, string config, PreValueCollection preValues, PropertyEditor editor)
        {
            if (value != null && value.ToString() != string.Empty)
            {
                var result = value.TryConvertTo<decimal>();
                if (result.Success == false)
                {
                    yield return new ValidationResult("The value " + value + " is not a valid decimal", new[] { "value" });
                }
            }
        }

        public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
        {
            return Validate(value, "", preValues, editor);
        }
    }
}