using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Core.PropertyEditors.Validators
{
    /// <summary>
    /// A validator that validates that the value is a valid decimal
    /// </summary>
    internal sealed class DecimalValidator : IManifestValueValidator
    {
        /// <inheritdoc />
        public string ValidationName => "Decimal";

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            if (value == null || value.ToString() == string.Empty)
                yield break;

            var result = value.TryConvertTo<decimal>();
            if (result.Success == false)
                yield return new ValidationResult("The value " + value + " is not a valid decimal", new[] { "value" });
        }
    }
}
