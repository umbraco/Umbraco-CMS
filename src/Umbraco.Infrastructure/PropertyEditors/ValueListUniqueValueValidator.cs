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
    private static readonly string[] _itemsMemberNames = ["items"];

    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueListUniqueValueValidator"/> class.
    /// </summary>
    /// <param name="configurationEditorJsonSerializer">An instance used to serialize and deserialize configuration editor values as JSON.</param>
    public ValueListUniqueValueValidator(IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        => _configurationEditorJsonSerializer = configurationEditorJsonSerializer;

    /// <summary>
    /// Validates that all values in the provided list are unique and that every option has a label.
    /// </summary>
    /// <param name="value">The value list options to validate, as a collection or as JSON. Each option is an object holding a value and a label.</param>
    /// <param name="valueType">The type of the value being validated (may be used for context).</param>
    /// <param name="dataTypeConfiguration">The configuration object for the data type (may be used for context).</param>
    /// <param name="validationContext">The context for property validation.</param>
    /// <returns>
    /// An <see cref="IEnumerable{ValidationResult}"/> containing a <see cref="ValidationResult"/> for each duplicate value and each missing label found in the list; returns an error if the value cannot be parsed as a list.
    /// If all values are unique, every option has a label, or the input is null, returns an empty sequence.
    /// </returns>
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        if (value is null)
        {
            yield break;
        }

        IEnumerable<ValueListOption>? options = null;
        try
        {
            options = value switch
            {
                IEnumerable<ValueListOption> typedOptions => typedOptions,
                string json => _configurationEditorJsonSerializer.Deserialize<ValueListOption[]>(json),
                _ => _configurationEditorJsonSerializer.Deserialize<ValueListOption[]>(_configurationEditorJsonSerializer.Serialize(value)),
            };
        }
        catch
        {
            // swallow and report error below
        }

        if (options is null)
        {
            yield return new ValidationResult($"The configuration value {value} is not a valid value list configuration", _itemsMemberNames);
            yield break;
        }

        var duplicateValues = options
            .Select(option => string.IsNullOrWhiteSpace(option.Value) ? string.Empty : option.Value)
            .GroupBy(v => v)
            .Where(group => group.Count() > 1)
            .Select(group => group.First())
            .ToArray();

        foreach (var duplicateValue in duplicateValues)
        {
            yield return new ValidationResult($"The value \"{duplicateValue}\" must be unique", _itemsMemberNames);
        }

        foreach (ValueListOption option in options.Where(option => string.IsNullOrWhiteSpace(option.Label)))
        {
            yield return new ValidationResult($"The value \"{option.Value}\" must have a label", _itemsMemberNames);
        }
    }
}
