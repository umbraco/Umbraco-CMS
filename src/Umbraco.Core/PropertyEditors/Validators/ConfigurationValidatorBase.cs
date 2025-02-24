using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
/// Provides common functionality to validators that rely on data type configuration.
/// </summary>
public abstract class ConfigurationValidatorBase
{
    /// <summary>
    /// Retrieves a typed value from data type configuration for the provided key.
    /// </summary>
    /// <param name="dataTypeConfiguration">The data type configuration.</param>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The configuration value (if found), otherwise zero.</param>
    /// <returns>True if the configured value was found.</returns>
    protected static bool TryGetConfiguredValue<TValue>(object? dataTypeConfiguration, string key, [NotNullWhen(true)] out TValue? value)
    {
        if (dataTypeConfiguration is not Dictionary<string, object> configuration)
        {
            value = default;
            return false;
        }

        if (configuration.TryGetValue(key, out object? obj) && obj is TValue castValue)
        {
            value = castValue
            return true;
        }

        value = default;
        return false;
    }
}
