// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a validator which ensures that all values in the list are unique.
/// </summary>
public class ValueListUniqueValueValidator : IValueValidator
{
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueListUniqueValueValidator"/> class.
    /// </summary>
    /// <param name="configurationEditorJsonSerializer">An instance used to serialize and deserialize configuration editor values as JSON.</param>
    public ValueListUniqueValueValidator(IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        => _configurationEditorJsonSerializer = configurationEditorJsonSerializer;

    /// <summary>
    /// Validates that all values in the provided list are unique.
    /// </summary>
    /// <param name="value">The value to validate, expected to be either an <see cref="IEnumerable{String}"/> or a JSON-serialized string array representing the list of values.</param>
    /// <param name="valueType">The type of the value being validated (may be used for context).</param>
    /// <param name="dataTypeConfiguration">The configuration object for the data type (may be used for context).</param>
    /// <param name="validationContext">The context for property validation.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing a <see cref="ValidationResult"/> for each duplicate value found in the list; returns an error if the value cannot be parsed as a list.
    /// If all values are unique or the input is null, returns an empty sequence.
    /// </returns>
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        if (value is null)
        {
            yield break;
        }

        var items = value as IEnumerable<string>;
        if (items is null)
        {
            try
            {
                items = _configurationEditorJsonSerializer.Deserialize<string[]>(value.ToString() ?? string.Empty);
            }
            catch
            {
                // swallow and report error below
            }
        }

        if (items is null)
        {
            yield return new ValidationResult($"The configuration value {value} is not a valid value list configuration", ["items"]);
            yield break;
        }

        var duplicateValues = items
            .Select(item => item)
            .GroupBy(v => v)
            .Where(group => group.Count() > 1)
            .Select(group => group.First())
            .ToArray();

        foreach (var duplicateValue in duplicateValues)
        {
            yield return new ValidationResult($"The value \"{duplicateValue}\" must be unique", new[] { "items" });
        }
    }
}
