using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///  Represents an entity data picker property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.EntityDataPicker,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
internal sealed class EntityDataPickerPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityDataPickerPropertyEditor" /> class.
    /// </summary>
    public EntityDataPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    public override IPropertyIndexValueFactory PropertyIndexValueFactory { get; } = new NoopPropertyIndexValueFactory();

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<EntityDataPickerPropertyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() => new EntityDataPickerConfigurationEditor(_ioHelper);

    /// <summary>
    /// Defines the value editor for the entity data picker property editor.
    /// </summary>
    internal sealed class EntityDataPickerPropertyValueEditor : DataValueEditor
    {
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDataPickerPropertyValueEditor"/> class.
        /// </summary>
        public EntityDataPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;

            var validators = new TypedJsonValidatorRunner<EntityDataPickerDto, EntityDataPickerConfiguration>(
                jsonSerializer,
                new MinMaxValidator(localizedTextService));

            Validators.Add(validators);
        }

        /// <inheritdoc/>
        public override IValueRequiredValidator RequiredValidator => new EntityDataPickerRequiredValidator(_jsonSerializer);

        /// <summary>
        /// Validates the min/max configuration for the entity data picker property editor.
        /// </summary>
        internal sealed class MinMaxValidator : ITypedJsonValidator<EntityDataPickerDto, EntityDataPickerConfiguration>
        {
            private readonly ILocalizedTextService _localizedTextService;

            /// <summary>
            /// Initializes a new instance of the <see cref="MinMaxValidator"/> class.
            /// </summary>
            public MinMaxValidator(ILocalizedTextService localizedTextService) =>
                _localizedTextService = localizedTextService;

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(
                EntityDataPickerDto? data,
                EntityDataPickerConfiguration? configuration,
                string? valueType,
                PropertyValidationContext validationContext)
            {
                var validationResults = new List<ValidationResult>();

                if (data is null || configuration is null)
                {
                    return validationResults;
                }

                if (ItemCountValidationHelper.IsBelowMinimum(data.Ids.Length, configuration.ValidationLimit.Min))
                {
                    validationResults.Add(new ValidationResult(
                        _localizedTextService.Localize(
                            "validation",
                            "entriesShort",
                            [configuration.ValidationLimit.Min.ToString(), (configuration.ValidationLimit.Min - data.Ids.Length).ToString()]),
                        ["value"]));
                }

                if (configuration.ValidationLimit.Max is not null
                    && data.Ids.Length > configuration.ValidationLimit.Max)
                {
                    validationResults.Add(new ValidationResult(
                        _localizedTextService.Localize(
                            "validation",
                            "entriesExceed",
                            [configuration.ValidationLimit.Max.ToString(), (data.Ids.Length - configuration.ValidationLimit.Max).ToString()
                            ]),
                        ["value"]));
                }

                return validationResults;
            }
        }
    }

    /// <summary>
    /// A custom <see cref="IValueRequiredValidator" /> that treats a value holding no selected entities as no value,
    /// which the default validator cannot detect as the value retains its surrounding structure once emptied.
    /// </summary>
    internal sealed class EntityDataPickerRequiredValidator : RequiredValidator
    {
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDataPickerRequiredValidator"/> class.
        /// </summary>
        public EntityDataPickerRequiredValidator(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

        /// <inheritdoc/>
        public override IEnumerable<ValidationResult> ValidateRequired(object? value, string? valueType)
        {
            IEnumerable<ValidationResult> validationResults = base.ValidateRequired(value, valueType);

            if (value is null)
            {
                return validationResults;
            }

            if (_jsonSerializer.TryDeserialize(value, out EntityDataPickerDto? dto) && dto.Ids.Length == 0)
            {
                validationResults = validationResults.Append(new ValidationResult(Constants.Validation.ErrorMessages.Properties.Empty, ["value"]));
            }

            return validationResults;
        }
    }

    /// <summary>
    ///     Represents the data transfer object for entity data picker values.
    /// </summary>
    internal sealed class EntityDataPickerDto
    {
        /// <summary>
        ///     Gets or sets the array of selected entity identifiers.
        /// </summary>
        public string[] Ids { get; set; } = [];
    }
}
