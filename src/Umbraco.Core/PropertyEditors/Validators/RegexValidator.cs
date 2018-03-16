using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.PropertyEditors.Validators
{
    /// <summary>
    /// A validator that validates that the value against a regular expression.
    /// </summary>
    internal sealed class RegexValidator : IValueFormatValidator, IManifestValueValidator
    {
        private string _regex;

        /// <inheritdoc cref="IManifestValueValidator.ValidationName"/>
        public string ValidationName => "Regex";

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexValidator"/> class.
        /// </summary>
        /// <remarks>Use this constructor when the validator is used as an <see cref="IValueFormatValidator"/>,
        /// and the regular expression is supplied at validation time. This constructor is also used when
        /// the validator is used as an <see cref="IManifestValueValidator"/> and the regular expression
        /// is supplied via the <see cref="Configure"/> method.</remarks>
        public RegexValidator()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexValidator"/> class.
        /// </summary>
        /// <remarks>Use this constructor when the validator is used as an <see cref="IValueValidator"/>,
        /// and the regular expression must be supplied when the validator is created.</remarks>
        public RegexValidator(string regex)
        {
            if (string.IsNullOrWhiteSpace(regex))
                throw new ArgumentNullOrEmptyException(nameof(regex));
            _regex = regex;
        }

        /// <summary>
        /// Gets or sets the configuration, when parsed as <see cref="IManifestValueValidator"/>.
        /// </summary>
        public string Configuration
        {
            get => _regex;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullOrEmptyException(nameof(value));
                _regex = value;
            }
        }

        /// <inheritdoc cref="IValueValidator.Validate"/>
        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            if (_regex == null)
                throw new InvalidOperationException("The validator has not been configured.");

            return ValidateFormat(value, valueType, _regex);
        }

        /// <inheritdoc cref="IValueFormatValidator.ValidateFormat"/>
        public IEnumerable<ValidationResult> ValidateFormat(object value, string valueType, string format)
        {
            if (string.IsNullOrWhiteSpace(format)) throw new ArgumentNullOrEmptyException(nameof(format));
            if (value == null || !new Regex(format).IsMatch(value.ToString()))
                yield return new ValidationResult("Value is invalid, it does not match the correct pattern", new[] { "value" });
        }
    }
}
