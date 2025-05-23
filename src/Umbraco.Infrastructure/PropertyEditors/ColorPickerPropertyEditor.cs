// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a color picker property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ColorPicker,
    ValueEditorIsReusable = true)]
public class ColorPickerPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorPickerPropertyEditor"/> class.
    /// </summary>
    public ColorPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        SupportsReadOnly = true;
    }

    /// <inheritdoc/>
    public override IPropertyIndexValueFactory PropertyIndexValueFactory { get; } = new NoopPropertyIndexValueFactory();

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<ColorPickerPropertyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ColorPickerConfigurationEditor(_ioHelper, _configurationEditorJsonSerializer);

    /// <summary>
    /// Defines the value editor for the color picker property editor.
    /// </summary>
    internal class ColorPickerPropertyValueEditor : DataValueEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPickerPropertyValueEditor"/> class.
        /// </summary>
        public ColorPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            Validators.AddRange(new ConfiguredColorValidator(localizedTextService));
        }

        /// <summary>
        /// Validates the color selection for the color picker property editor.
        /// </summary>
        internal class ConfiguredColorValidator : IValueValidator
        {
            private readonly ILocalizedTextService _localizedTextService;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConfiguredColorValidator"/> class.
            /// </summary>
            public ConfiguredColorValidator(ILocalizedTextService localizedTextService) => _localizedTextService = localizedTextService;

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
            {
                if (value is null || value is not JsonObject valueAsJsonObject)
                {
                    yield break;
                }

                if (dataTypeConfiguration is not ColorPickerConfiguration colorPickerConfiguration)
                {
                    yield break;
                }

                string? selectedColor = valueAsJsonObject["value"]?.GetValue<string>();
                if (selectedColor.IsNullOrWhiteSpace())
                {
                    yield break;
                }

                IEnumerable<string> validColors = colorPickerConfiguration.Items.Select(x => EnsureConsistentColorRepresentation(x.Value));
                if (validColors.Contains(EnsureConsistentColorRepresentation(selectedColor)) is false)
                {
                    yield return new ValidationResult(
                        _localizedTextService.Localize("validation", "invalidColor", [selectedColor]),
                        ["value"]);
                }
            }

            private static string EnsureConsistentColorRepresentation(string color)
                => (color.StartsWith('#') ? color : $"#{color}").ToLowerInvariant();
        }
    }
}
