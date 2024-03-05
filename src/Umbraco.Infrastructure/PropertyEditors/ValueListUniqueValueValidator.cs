// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
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

    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
    {
        string[]? items = null;

        switch (value)
        {
            case string stringValue:
                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    try
                    {
                        items = _configurationEditorJsonSerializer.Deserialize<string[]>(stringValue);
                    }
                    catch
                    {
                        // swallow and report error below
                    }
                }
                break;

            case string[] stringArray:
                items = stringArray;
                break;

            case List<string> stringList:
                items = stringList.ToArray();
                break;

            case IEnumerable<string> stringEnumerable:
                items = stringEnumerable.ToArray();
                break;

            default:
                break;
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
