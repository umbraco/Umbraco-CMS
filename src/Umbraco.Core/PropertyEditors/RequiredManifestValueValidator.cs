using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator that validates that the value is not null or empty (if it is a string)
    /// </summary>
    internal sealed class RequiredManifestValueValidator : ManifestValidator
    {
        /// <inheritdoc />
        public override string ValidationName => "Required";

        public override IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration, object validatorConfiguration)
        {
            //TODO: localize these!

            if (value == null)
            {
                yield return new ValidationResult("Value cannot be null", new[] {"value"});
            }
            else
            {
                var asString = value.ToString();

                if (valueType.InvariantEquals(ValueTypes.Json))
                {
                    if (asString.DetectIsEmptyJson())
                    {
                        yield return new ValidationResult("Value cannot be empty", new[] { "value" });
                    }
                }

                if (asString.IsNullOrWhiteSpace())
                {
                    yield return new ValidationResult("Value cannot be empty", new[] { "value" });
                }
            }


        }
    }
}
