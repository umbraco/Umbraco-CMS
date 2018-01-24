using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a value validator that implements a named validation for manifets.
    /// </summary>
    public abstract class ManifestValidator
    {
        /// <summary>
        /// Gets the validation name of this validator.
        /// </summary>
        public abstract string ValidationName { get; }

        /// <summary>
        /// Validates a value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="valueType">The expected <see cref="ValueTypes"/>.</param>
        /// <param name="dataTypeConfiguration">The datatype configuration.</param>
        /// <param name="validatorConfiguration">The validator configuration, defined in the manifest.</param>
        /// <returns>Validation results.</returns>
        /// <remarks>
        /// <para>The value can be a string, a Json structure (JObject, JArray...)... corresponding to what was posted by an editor.</para>
        /// </remarks>
        public abstract IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration, object validatorConfiguration);
    }
}
