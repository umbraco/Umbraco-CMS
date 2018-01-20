using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Core.PropertyEditors.Validators
{
    /// <summary>
    /// A validator that validates an email address
    /// </summary>
    [ValueValidator("Email")]
    internal sealed class EmailValidator : ManifestValueValidator, IPropertyValidator
    {
        /// <inheritdoc />
        public override IEnumerable<ValidationResult> Validate(object value, string validatorConfiguration, object dataTypeConfiguration, PropertyEditor editor)
        {
            var asString = value.ToString();

            var emailVal = new EmailAddressAttribute();

            if (asString != string.Empty && emailVal.IsValid(asString) == false)
            {
                // TODO: localize these!
                yield return new ValidationResult("Email is invalid", new[] { "value" });
            }
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(object value, object dataTypeConfiguration, PropertyEditor editor)
        {
            return this.Validate(value, null, dataTypeConfiguration, editor);
        }
    }
}
