using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
/// Provides common functionality to validators that rely on data type configuration.
/// </summary>
/// <typeparam name="TValue">The type to parse to.</typeparam>
public abstract class SimplePropertyConfigurationValidatorBase<TValue> : DictionaryConfigurationValidatorBase
{
    /// <summary>
    /// Parses the raw property value into it's typed equivalent.
    /// </summary>
    /// <param name="value">The property value as a nullable object.</param>
    /// <param name="parsedValue">The parsed value.</param>
    /// <returns>True if the parse succeeded, otherwise false.</returns>
    protected abstract bool TryParsePropertyValue(object? value, out TValue parsedValue);
}
