// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.DateTime2,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class DateTime2PropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;
    private readonly IDateTime2PropertyIndexValueFactory _propertyIndexValueFactory;

    public DateTime2PropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IDateTime2PropertyIndexValueFactory propertyIndexValueFactory)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _propertyIndexValueFactory = propertyIndexValueFactory;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new DateTime2ConfigurationEditor(_ioHelper);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<DateTime2DataValueEditor>(Attribute!);

    /// <inheritdoc/>
    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _propertyIndexValueFactory;

    internal class DateTime2DataValueEditor : DataValueEditor
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

        public DateTime2DataValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService,
            IDataTypeConfigurationCache dataTypeConfigurationCache)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
            _dataTypeConfigurationCache = dataTypeConfigurationCache;
            var validators = new TypedJsonValidatorRunner<DateTime2ApiModel, DateTime2Configuration>(
                jsonSerializer,
                new DateTime2Validator(localizedTextService));

            Validators.Add(validators);
        }

        /// <inheritdoc/>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            if (editorValue.Value is null ||
                _jsonSerializer.TryDeserialize(editorValue.Value, out DateTime2ApiModel? dateTime2ApiModel) is false)
            {
                return base.FromEditor(editorValue, currentValue);
            }

            var selectedDate = dateTime2ApiModel.Date;
            if (selectedDate.IsNullOrWhiteSpace()
                || !DateTimeOffset.TryParse(selectedDate, null, DateTimeStyles.AssumeUniversal, out DateTimeOffset dateTimeOffset))
            {
                return null;
            }

            var configuration = editorValue.DataTypeConfiguration as DateTime2Configuration;
            if (configuration?.Format == DateTime2Configuration.DateTimeFormat.TimeOnly)
            {
                // Clear the date part if the format is TimeOnly.
                // This is needed because `DateTimeOffset.TryParse` does not support `DateTimeStyles.NoCurrentDateDefault`.
                dateTimeOffset = new DateTimeOffset(DateOnly.MinValue, TimeOnly.FromTimeSpan(dateTimeOffset.TimeOfDay), dateTimeOffset.Offset);
            }

            var value = new DateTime2ValueConverter.DateTime2Dto
            {
                Date = dateTimeOffset,
                TimeZone = dateTime2ApiModel.TimeZone,
            };

            return _jsonSerializer.Serialize(value);
        }

        /// <inheritdoc/>
        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);

            DateTime2ValueConverter.DateTime2Dto? interValue = DateTime2ValueConverter.GetIntermediateFromSource(value, _jsonSerializer);
            if (interValue is null)
            {
                return null;
            }

            DateTime2Configuration? configuration = GetConfiguration(property.PropertyType.DataTypeKey);
            var objectValue = DateTime2ValueConverter.GetObjectFromIntermediate(interValue, configuration);

            return new DateTime2ApiModel
            {
                Date = objectValue is null ? null : $"{objectValue:O}",
                TimeZone = interValue.TimeZone,
            };
        }

        private DateTime2Configuration? GetConfiguration(Guid dataTypeKey) =>
            _dataTypeConfigurationCache.GetConfigurationAs<DateTime2Configuration>(dataTypeKey);

        /// <summary>
        /// Validates the date time selection for the DateTime2 property editor.
        /// </summary>
        private class DateTime2Validator : ITypedJsonValidator<DateTime2ApiModel, DateTime2Configuration>
        {
            private readonly ILocalizedTextService _localizedTextService;

            public DateTime2Validator(ILocalizedTextService localizedTextService)
                => _localizedTextService = localizedTextService;

            public IEnumerable<ValidationResult> Validate(
                DateTime2ApiModel? value,
                DateTime2Configuration? configuration,
                string? valueType,
                PropertyValidationContext validationContext)
            {
                if (value is null)
                {
                    yield break;
                }

                if (value.Date is null || DateTimeOffset.TryParse(value.Date, out DateTimeOffset _) is false)
                {
                    yield return new ValidationResult(
                        _localizedTextService.Localize("validation", "invalidNull"),
                        ["value"]);
                }

                if (configuration?.TimeZones?.Mode is not { } mode || mode == DateTime2Configuration.TimeZoneMode.None)
                {
                    yield break;
                }

                if (mode == DateTime2Configuration.TimeZoneMode.Custom
                    && configuration.TimeZones.TimeZones.Any(t => t.Equals(value.TimeZone, StringComparison.InvariantCultureIgnoreCase)) != true)
                {
                    yield return new ValidationResult(
                        _localizedTextService.Localize("validation", "notOneOfOptions", [value.TimeZone]),
                        ["value"]);
                }
            }
        }
    }

    internal class DateTime2ApiModel
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; set; }
    }
}
