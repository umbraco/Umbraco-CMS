using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a validator referenced in a package manifest.
    /// </summary>
    internal class ManifestValueValidator : IValueValidator
    {
        private readonly ManifestValidatorCollection _validators;
        private ManifestValidator _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestValueValidator"/> class.
        /// </summary>
        public ManifestValueValidator(ManifestValidatorCollection validators)
        {
            _validators = validators;
        }

        /// <summary>
        /// Gets or sets the name of the validation.
        /// </summary>
        [JsonProperty("type", Required = Required.Always)]
        public string ValidationName { get; set; }

        /// <summary>
        /// The configuration defined for this validator in the manifest.
        /// </summary>
        /// <remarks>
        /// <para>This has nothing to do with datatype configuration.</para>
        /// <para>The value is deserialized Json, can be a string or a Json thing (JObject...).</para>
        /// </remarks>
        [JsonProperty("config")]
        public object Config { get; set; }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            if (_validator == null)
            {
                _validator = _validators[ValidationName];
                if (_validator == null)
                    throw new InvalidOperationException($"No manifest validator exists for validation name \"{ValidationName}\".");
            }

            // validates the value, using the manifest validator 
            return _validator.Validate(value, valueType, Config, dataTypeConfiguration);
        }
    }
}
