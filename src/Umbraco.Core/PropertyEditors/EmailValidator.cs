namespace Umbraco.Core.PropertyEditors
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using umbraco;

    using Umbraco.Core.Models;

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
                yield return new ValidationResult(ui.Text("errors", "emailIsInvalid"), new[] { "value" });
            }
        }

        public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
        {
            return this.Validate(value, null, preValues, editor);
        }
    }
}