using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Umbraco.Core.PropertyEditors.Validators
{
    /// <summary>
    /// A validator that validates that the value against a Regex expression
    /// </summary>
    internal sealed class RegexValidator : ManifestValidator, IValueValidator
    {
        private readonly string _regex;

        /// <inheritdoc />
        public override string ValidationName => "Regex";

        /// <summary>
        /// Normally used when configured as a ManifestValueValidator
        /// </summary>
        public RegexValidator()
        { }

        /// <summary>
        /// Normally used when configured as an IPropertyValidator
        /// </summary>
        /// <param name="regex"></param>
        public RegexValidator(string regex)
        {
            _regex = regex ?? throw new ArgumentNullException(nameof(regex));
        }

        /// <inheritdoc />
        public override IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration, object validatorConfiguration)
        {
            //TODO: localize these!
            if (validatorConfiguration is string regexSource && !string.IsNullOrWhiteSpace(regexSource) && value != null)
            {
                var asString = value.ToString();

                var regex = new Regex(regexSource);

                if (regex.IsMatch(asString) == false)
                {
                    yield return new ValidationResult("Value is invalid, it does not match the correct pattern", new[] { "value" });
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            if (_regex == null)
            {
                throw new InvalidOperationException("This validator is not configured as a " + typeof(IValueValidator));
            }
            return Validate(value, valueType, dataTypeConfiguration, _regex);
        }
    }
}
