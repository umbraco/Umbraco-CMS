using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Umbraco.Core.PropertyEditors.Validators
{
    /// <summary>
    /// A validator that validates that the value against a Regex expression
    /// </summary>
    [ValueValidator("Regex")]
    internal sealed class RegexValidator : ManifestValueValidator, IPropertyValidator
    {
        private readonly string _regex;

        /// <summary>
        /// Normally used when configured as a ManifestValueValidator
        /// </summary>
        public RegexValidator()
        {
        }

        /// <summary>
        /// Normally used when configured as an IPropertyValidator
        /// </summary>
        /// <param name="regex"></param>
        public RegexValidator(string regex)
        {
            _regex = regex ?? throw new ArgumentNullException(nameof(regex));
        }

        public override IEnumerable<ValidationResult> Validate(object value, string validatorConfiguration, object dataTypeConfiguration, PropertyEditor editor)
        {
            //TODO: localize these!
            if (validatorConfiguration.IsNullOrWhiteSpace() == false && value != null)
            {
                var asString = value.ToString();

                var regex = new Regex(validatorConfiguration);

                if (regex.IsMatch(asString) == false)
                {
                    yield return new ValidationResult("Value is invalid, it does not match the correct pattern", new[] { "value" });
                }
            }

        }

        /// <summary>
        /// Used when configured as an IPropertyValidator
        /// </summary>
        /// <param name="value"></param>
        /// <param name="preValues"></param>
        /// <param name="editor"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(object value, object dataTypeConfiguration, PropertyEditor editor)
        {
            if (_regex == null)
            {
                throw new InvalidOperationException("This validator is not configured as a " + typeof(IPropertyValidator));
            }
            return Validate(value, _regex, dataTypeConfiguration, editor);
        }
    }
}
