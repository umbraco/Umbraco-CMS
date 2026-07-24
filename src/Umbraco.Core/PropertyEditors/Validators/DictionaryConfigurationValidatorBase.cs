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

    /// <summary>
    /// Retrieves the minimum and maximum bounds of a range value from data type dictionary configuration.
    /// </summary>
    /// <param name="dataTypeConfiguration">The data type configuration.</param>
    /// <param name="key">The configuration key holding the range value.</param>
    /// <param name="min">The minimum bound, or <c>null</c> when unbounded or not present.</param>
    /// <param name="max">The maximum bound, or <c>null</c> when unbounded or not present.</param>
    /// <returns>True if the range configuration value was found.</returns>
    protected static bool TryGetConfiguredRange(object? dataTypeConfiguration, string key, out decimal? min, out decimal? max)
    {
        min = null;
        max = null;

        return dataTypeConfiguration is IDictionary<string, object> configuration
            && configuration.TryGetValue(key, out object? rangeValue)
            && RangeConfigurationHelper.TryGetBounds(rangeValue, out min, out max);
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
