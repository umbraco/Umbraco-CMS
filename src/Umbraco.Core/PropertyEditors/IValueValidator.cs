using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Defines a value validator.
/// </summary>
public interface IValueValidator
{
    /// <summary>
    ///     Validates a value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="valueType">The value type.</param>
    /// <param name="dataTypeConfiguration">A datatype configuration.</param>
    /// <param name="validationContext">The context in which the value is being validated.</param>
    /// <returns>Validation results.</returns>
    /// <remarks>
    ///     <para>
    ///         The value can be a string, a Json structure (JObject, JArray...)... corresponding to what was posted by an
    ///         editor.
    ///     </para>
    /// </remarks>
    IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext);
}
