// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.DateTimeWithTimeZone,
    ValueEditorIsReusable = true)]
public class DateTimeWithTimeZonePropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;
    private readonly IDateTimeWithTimeZonePropertyIndexValueFactory _propertyIndexValueFactory;

    public DateTimeWithTimeZonePropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IDateTimeWithTimeZonePropertyIndexValueFactory propertyIndexValueFactory)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _propertyIndexValueFactory = propertyIndexValueFactory;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new DateWithTimeZoneConfigurationEditor(_ioHelper);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<DateTimeWithTimeZoneDataValueEditor>(Attribute!);

    /// <inheritdoc/>
    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _propertyIndexValueFactory;

    private class DateTimeWithTimeZoneDataValueEditor : DataValueEditor
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

        public DateTimeWithTimeZoneDataValueEditor(
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
            Validators.AddRange(new DateTimeWithTimeZoneValidator(localizedTextService));
        }

        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            if (editorValue.Value is not JsonObject valueAsJsonObject)
            {
                return null;
            }

            var selectedDate = valueAsJsonObject["date"]?.GetValue<string>();
            if (selectedDate.IsNullOrWhiteSpace())
            {
                return null;
            }

            if (!DateTimeOffset.TryParse(selectedDate, null, DateTimeStyles.AssumeUniversal, out DateTimeOffset dateTimeOffset))
            {
                return null;
            }

            var configuration = editorValue.DataTypeConfiguration as DateWithTimeZoneConfiguration;
            if (configuration?.Format == DateWithTimeZoneFormat.TimeOnly)
            {
                // Clear the date part if the format is TimeOnly.
                // This is needed because `DateTimeOffset.TryParse` does not support `DateTimeStyles.NoCurrentDateDefault`.
                dateTimeOffset = new DateTimeOffset(DateOnly.MinValue, TimeOnly.FromTimeSpan(dateTimeOffset.TimeOfDay), dateTimeOffset.Offset);
            }

            var value = new DateTimeWithTimeZoneValueConverter.DateWithTimeZone
            {
                Date = dateTimeOffset,
                TimeZone = valueAsJsonObject["timeZone"]?.GetValue<string>(),
            };

            var jsonStr = _jsonSerializer.Serialize(value);
            return jsonStr;
        }

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);
            if (value is not string valueString)
            {
                return null;
            }

            var interValue = DateTimeWithTimeZoneValueConverter.GetIntermediateValue(valueString, _jsonSerializer);
            if (interValue is not DateTimeWithTimeZoneValueConverter.DateWithTimeZone dateWithTimeZone)
            {
                return null;
            }

            DateWithTimeZoneConfiguration? configuration = _dataTypeConfigurationCache.GetConfigurationAs<DateWithTimeZoneConfiguration>(property.PropertyType.DataTypeKey);
            var objectValue = DateTimeWithTimeZoneValueConverter.GetObjectValue(dateWithTimeZone, configuration);

            JsonNode node = new JsonObject();
            if (objectValue is DateTimeOffset && dateWithTimeZone.TimeZone is null)
            {
                // If the time zone is not set, we assume the date is in UTC.
                node["timeZone"] = "UTC";
            }
            else if (objectValue is not null && objectValue is not DateTimeOffset)
            {
                // If the value is not a DateTimeOffset, clean the time zone.
                node["timeZone"] = null;
            }
            else
            {
                node["timeZone"] = dateWithTimeZone.TimeZone;
            }

            node["date"] = DateTimeWithTimeZoneValueConverter.GetDateValueAsString(objectValue);

            return node;
        }

        /// <summary>
        /// Validates the color selection for the color picker property editor.
        /// </summary>
        private class DateTimeWithTimeZoneValidator : IValueValidator
        {
            private readonly ILocalizedTextService _localizedTextService;

            public DateTimeWithTimeZoneValidator(ILocalizedTextService localizedTextService) => _localizedTextService = localizedTextService;

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(
                object? value,
                string? valueType,
                object? dataTypeConfiguration,
                PropertyValidationContext validationContext)
            {
                if (value is not JsonObject valueAsJsonObject)
                {
                    yield break;
                }

                var selectedDate = valueAsJsonObject["date"]?.GetValue<string>();
                if (selectedDate.IsNullOrWhiteSpace() || !DateTimeOffset.TryParse(selectedDate, out DateTimeOffset _))
                {
                    yield return new ValidationResult(
                        _localizedTextService.Localize("validation", "invalidDate", [selectedDate]),
                        ["value"]);
                }

                var selectedTimeZone = valueAsJsonObject["timeZone"]?.GetValue<string>();
                var dataTypeConfig = dataTypeConfiguration as DateWithTimeZoneConfiguration;
                if (dataTypeConfig?.TimeZones?.Mode is not { } mode || mode == DateWithTimeZoneMode.None)
                {
                    yield break;
                }

                if (mode == DateWithTimeZoneMode.Custom
                    && dataTypeConfig.TimeZones.TimeZones.Any(t => t.Equals(selectedTimeZone, StringComparison.InvariantCultureIgnoreCase)) != true)
                {
                    yield return new ValidationResult(
                        _localizedTextService.Localize("validation", "invalidTimeZone", [selectedTimeZone]),
                        ["value"]);
                }
            }
        }
    }
}
