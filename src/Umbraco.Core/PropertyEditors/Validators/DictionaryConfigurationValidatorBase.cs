using System.Diagnostics.CodeAnalysis;
namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
/// Provides common functionality to validators that rely on data type configuration.
/// </summary>
public abstract class DictionaryConfigurationValidatorBase
{
    /// <summary>
    /// Retrieves a typed value from data type dictionary configuration for the provided key.
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

        if (configuration.TryGetValue(key, out object? obj) && TryCastValue(obj, out TValue? castValue))
        {
            value = castValue;
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryCastValue<TValue>(object? value, [NotNullWhen(true)] out TValue? castValue)
    {
        if (value is TValue valueAsType)
        {
            castValue = valueAsType;
            return true;
        }

        // Special case for floating point numbers - when deserialized these will be integers if whole numbers rather
        // than double.
        if (typeof(TValue) == typeof(double) && value is int valueAsInt)
        {
            castValue = (TValue)(object)Convert.ToDouble(valueAsInt);
            return true;
        }

        castValue = default;
        return false;
    }
}
