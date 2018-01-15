using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Defines a property validator.
    /// </summary>
    public interface IPropertyValidator
    {
        /// <summary>
        /// Validates.
        /// </summary>
        /// <param name="value">The value to validate. Can be a json structure (JObject, JArray, etc...), could be a single string, representing an editor's model.</param>
        /// <param name="dataTypeConfiguration">The data type configuration.</param>
        /// <param name="editor">The property editor.</param>
        /// <returns>Validation results.</returns>
        /// <remarks>If a validation result does not have a field name, then it applies to the entire property
        /// type being validated. Otherwise, it matches a field.</remarks>
        IEnumerable<ValidationResult> Validate(object value, object dataTypeConfiguration, PropertyEditor editor);
    }
}
