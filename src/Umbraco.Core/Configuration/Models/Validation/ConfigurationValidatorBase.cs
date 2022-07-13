// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Base class for configuration validators.
/// </summary>
public abstract class ConfigurationValidatorBase
{
    /// <summary>
    ///     Validates that a string is one of a set of valid values.
    /// </summary>
    /// <param name="configPath">Configuration path from where the setting is found.</param>
    /// <param name="value">The value to check.</param>
    /// <param name="validValues">The set of valid values.</param>
    /// <param name="message">A message to output if the value does not match.</param>
    /// <returns>True if valid, false if not.</returns>
    public bool ValidateStringIsOneOfValidValues(string configPath, string value, IEnumerable<string> validValues, out string message)
    {
        if (!validValues.InvariantContains(value))
        {
            message =
                $"Configuration entry {configPath} contains an invalid value '{value}', it should be one of the following: '{string.Join(", ", validValues)}'.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    /// <summary>
    ///     Validates that a collection of objects are all valid based on their data annotations.
    /// </summary>
    /// <param name="configPath">Configuration path from where the setting is found.</param>
    /// <param name="values">The values to check.</param>
    /// <param name="validationDescription">Description of validation appended to message if validation fails.</param>
    /// <param name="message">A message to output if the value does not match.</param>
    /// <returns>True if valid, false if not.</returns>
    public bool ValidateCollection(string configPath, IEnumerable<ValidatableEntryBase> values, string validationDescription, out string message)
    {
        if (values.Any(x => !x.IsValid()))
        {
            message = $"Configuration entry {configPath} contains one or more invalid values. {validationDescription}.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    /// <summary>
    ///     Validates a configuration entry is valid if provided.
    /// </summary>
    /// <param name="configPath">Configuration path from where the setting is found.</param>
    /// <param name="value">The value to check.</param>
    /// <param name="validationDescription">Description of validation appended to message if validation fails.</param>
    /// <param name="message">A message to output if the value does not match.</param>
    /// <returns>True if valid, false if not.</returns>
    public bool ValidateOptionalEntry(string configPath, ValidatableEntryBase? value, string validationDescription, out string message)
    {
        if (value != null && !value.IsValid())
        {
            message = $"Configuration entry {configPath} contains one or more invalid values. {validationDescription}.";
            return false;
        }

        message = string.Empty;
        return true;
    }
}
