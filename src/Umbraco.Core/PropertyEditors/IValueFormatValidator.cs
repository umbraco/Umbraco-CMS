using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Defines a value format validator.
    /// </summary>
    public interface IValueFormatValidator
    {
        /// <summary>
        /// Validates a value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="valueType">The value type.</param>
        /// <param name="format">A format definition.</param>
        /// <param name="formatMessage">A custom validation message to use when the property value is does not match the specific format (regex).</param>
        /// <returns>Validation results.</returns>
        /// <remarks>
        /// <para>The <paramref name="format" /> is expected to be a valid regular expression.</para>
        /// <para>This is used to validate values against the property type validation regular expression.</para>
        /// </remarks>
        IEnumerable<ValidationResult> ValidateFormat(object value, string valueType, string format, string formatMessage);
    }
}
