// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class ColorPickerConfigurationEditor : ConfigurationEditor<ColorPickerConfiguration>
{
    public ColorPickerConfigurationEditor(IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(ioHelper)
    {
        ConfigurationField items = Fields.First(x => x.Key == "items");
        items.Validators.Add(new ColorListValidator(configurationEditorJsonSerializer));
    }

    internal class ColorListValidator : IValueValidator
    {
        private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

        public ColorListValidator(IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
            => _configurationEditorJsonSerializer = configurationEditorJsonSerializer;

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
                yield return new ValidationResult($"The configuration value {stringValue} is not a valid color picker configuration", new[] { "items" });
                yield break;
            }

            foreach (ColorPickerConfiguration.ColorPickerItem item in items)
            {
                if (Regex.IsMatch(item.Value, "^([0-9a-f]{3}|[0-9a-f]{6})$", RegexOptions.IgnoreCase) == false)
                {
                    yield return new ValidationResult($"The value {item.Value} is not a valid hex color", new[] { "items" });
                }
            }
        }
    }
}
