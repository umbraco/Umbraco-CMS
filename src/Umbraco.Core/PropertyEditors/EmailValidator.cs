using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator that validates an email address
    /// </summary>
    [ValueValidator("Email")]
    internal sealed class EmailValidator : ManifestValueValidator, IPropertyValidator
    {
        public override IEnumerable<ValidationResult> Validate(object value, string config, PreValueCollection preValues, PropertyEditor editor)
        {
            var asString = value.ToString();

            var emailVal = new EmailAddressAttribute();

            if (asString != string.Empty && emailVal.IsValid(asString) == false)
            {
                // TODO: localize these!
                yield return new ValidationResult("Email is invalid", new[] { "value" });
            }
        }

        public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
        {
            return this.Validate(value, null, preValues, editor);
        }
    }
}