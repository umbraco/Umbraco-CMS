using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors.Validators
{
    /// <summary>
    /// A validator that validates that the value is not null or empty (if it is a string)
    /// </summary>
    internal sealed class RequiredValidator : IValueRequiredValidator, IManifestValueValidator
    {
        private readonly ILocalizedTextService _textService;

        public RequiredValidator() : this(Current.Services.TextService)
        {
        }

        public RequiredValidator(ILocalizedTextService textService)
        {
            _textService = textService;
        }

        /// <inheritdoc cref="IManifestValueValidator.ValidationName"/>
        public string ValidationName => "Required";

        /// <inheritdoc cref="IValueValidator.Validate"/>
        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            return ValidateRequired(value, valueType);
        }

        /// <inheritdoc cref="IValueRequiredValidator.ValidateRequired"/>
        public IEnumerable<ValidationResult> ValidateRequired(object value, string valueType)
        {
            if (value == null)
            {
                yield return new ValidationResult(_textService.Localize("validation", "invalidNull"), new[] {"value"});
                yield break;
            }

            if (valueType.InvariantEquals(ValueTypes.Json))
            {
                if (value.ToString().DetectIsEmptyJson())
                    yield return new ValidationResult(_textService.Localize("validation", "invalidEmpty"), new[] { "value" });
                yield break;
            }

            if (value.ToString().IsNullOrWhiteSpace())
            {
                yield return new ValidationResult(_textService.Localize("validation", "invalidEmpty"), new[] { "value" });
            }
        }
    }
}
