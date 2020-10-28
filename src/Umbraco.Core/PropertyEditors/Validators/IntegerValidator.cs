using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.PropertyEditors.Validators
{
    /// <summary>
    /// A validator that validates that the value is a valid integer
    /// </summary>
    [UmbracoVolatile]
    public sealed class IntegerValidator : IManifestValueValidator
    {
        /// <inheritdoc />
        public string ValidationName => "Integer";

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
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
    }
}
