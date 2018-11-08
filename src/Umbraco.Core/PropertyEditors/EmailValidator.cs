using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
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

            // This is not the ffficial RFC 5322 regular expression, but it's a version which comes pretty close to. (Inspired by: https://www.regular-expressions.info/email.html)
            var emailSyntax = @"^[a-z0-9][a-z0-9!#$%&'*+\/=?^_`{|}~.-]*@([a-z0-9]([a-z0-9-]*)\.)*([a-z0-9]([a-z0-9-]*[a-z0-9]))+$";
            var isMatch = Regex.IsMatch(asString, emailSyntax, RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(asString) && !isMatch)
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
