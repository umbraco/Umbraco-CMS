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

    public ValueListUniqueValueValidator(IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        => _configurationEditorJsonSerializer = configurationEditorJsonSerializer;

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
