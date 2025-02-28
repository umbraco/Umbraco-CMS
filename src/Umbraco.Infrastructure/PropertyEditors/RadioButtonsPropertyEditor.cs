// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A property editor to allow the individual selection of pre-defined items.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.RadioButtonList,
    ValueType = ValueTypes.String,
    ValueEditorIsReusable = true)]
public class RadioButtonsPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioButtonsPropertyEditor"/> class.
    /// </summary>
    public RadioButtonsPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        SupportsReadOnly = true;
    }

    /// <inheritdoc/>

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ValueListConfigurationEditor(_ioHelper, _configurationEditorJsonSerializer);

    /// <inheritdoc/>
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<RadioButtonsPropertyValueEditor>(Attribute!);


    /// <summary>
    /// Defines the value editor for the radio buttons property editor.
    /// </summary>
    internal class RadioButtonsPropertyValueEditor : DataValueEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadioButtonsPropertyValueEditor"/> class.
        /// </summary>
        public RadioButtonsPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
            => Validators.AddRange([new RadioButtonValueValidator(localizedTextService)]);
    }

    /// <summary>
    /// Validates the prevalue configuration for the radio buttons property editor.
    /// </summary>
    internal class RadioButtonValueValidator : IValueValidator
    {
        private readonly ILocalizedTextService _localizedTextService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RadioButtonValueValidator"/> class.
        /// </summary>
        /// <param name="localizedTextService"></param>
        public RadioButtonValueValidator(ILocalizedTextService localizedTextService) => _localizedTextService = localizedTextService;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
        {
            if (value == null || value.ToString().IsNullOrWhiteSpace())
            {
                yield break;
            }

            if (dataTypeConfiguration is not ValueListConfiguration valueListConfiguration)
            {
                yield break;
            }

            if (value is not string valueAsString)
            {
                yield break;
            }

            if (valueListConfiguration.Items.Contains(valueAsString) is false)
            {
                yield return new ValidationResult(
                    _localizedTextService.Localize("validation", "notOneOfOptions", [valueAsString.ToString()]),
                    ["value"]);
            }
        }
    }
}
