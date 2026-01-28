// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed partial class ColorPickerConfigurationEditor : ConfigurationEditor<ColorPickerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorPickerConfigurationEditor"/> class.
    /// </summary>
    public ColorPickerConfigurationEditor(IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(ioHelper)
    {
        ConfigurationField items = Fields.First(x => x.Key == "items");
        items.Validators.Add(new ColorListValidator(configurationEditorJsonSerializer));
    }

    internal sealed partial class ColorListValidator : IValueValidator
    {
        private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorListValidator"/> class.
        /// </summary>
        public ColorListValidator(IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
            => _configurationEditorJsonSerializer = configurationEditorJsonSerializer;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
        {
            var stringValue = value?.ToString();
            if (stringValue.IsNullOrWhiteSpace())
            {
                yield break;
            }

            ColorPickerConfiguration.ColorPickerItem[]? items = null;
            try
            {
                items = _configurationEditorJsonSerializer.Deserialize<ColorPickerConfiguration.ColorPickerItem[]>(stringValue);
            }
            catch
            {
                // swallow and report error below
            }

            if (items is null)
            {
                yield return new ValidationResult($"The configuration value {stringValue} is not a valid color picker configuration", ["items"]);
                yield break;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var duplicates = new List<string>();
            foreach (ColorPickerConfiguration.ColorPickerItem item in items)
            {
                if (ColorPattern().IsMatch(item.Value) == false)
                {
                    yield return new ValidationResult($"The value {item.Value} is not a valid hex color", ["items"]);
                    continue;
                }

                var normalized = Normalize(item.Value);
                if (seen.Add(normalized) is false)
                {
                    duplicates.Add(normalized);
                }
            }

            if (duplicates.Count > 0)
            {
                yield return new ValidationResult(
                    $"Duplicate color values are not allowed: {string.Join(", ", duplicates)}",
                    ["items"]);
            }
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalizedValue = value.Trim().ToLowerInvariant();

            if (normalizedValue.Length == 3)
            {
                normalizedValue = $"{normalizedValue[0]}{normalizedValue[0]}{normalizedValue[1]}{normalizedValue[1]}{normalizedValue[2]}{normalizedValue[2]}";
            }

            return normalizedValue;
        }

        [GeneratedRegex("^([0-9a-f]{3}|[0-9a-f]{6}|[0-9a-f]{8})$", RegexOptions.IgnoreCase, "en-GB")]
        private static partial Regex ColorPattern();
    }
}
