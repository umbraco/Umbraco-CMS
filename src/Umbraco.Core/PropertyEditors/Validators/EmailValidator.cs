using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Core.PropertyEditors.Validators
{
    /// <summary>
    /// A validator that validates an email address
    /// </summary>
    internal sealed class EmailValidator : ManifestValidator, IValueValidator
    {
        /// <inheritdoc />
        public override string ValidationName => "Email";

        /// <inheritdoc />
        public override IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration, object validatorConfiguration)
        {
            return Validate(value, valueType, dataTypeConfiguration);
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            var asString = value.ToString();

            var emailVal = new EmailAddressAttribute();

            if (asString != string.Empty && emailVal.IsValid(asString) == false)
            {
                // TODO: localize these!
                yield return new ValidationResult("Email is invalid", new[] { "value" });
            }
        }
    }
}
