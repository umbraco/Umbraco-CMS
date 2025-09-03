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
            Validators.AddRange(new DateTime2Validator(localizedTextService));
        }

        /// <inheritdoc/>
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

            var configuration = editorValue.DataTypeConfiguration as DateTime2Configuration;
            if (configuration?.Format == DateTime2Configuration.DateTimeFormat.TimeOnly)
            {
                // Clear the date part if the format is TimeOnly.
                // This is needed because `DateTimeOffset.TryParse` does not support `DateTimeStyles.NoCurrentDateDefault`.
                dateTimeOffset = new DateTimeOffset(DateOnly.MinValue, TimeOnly.FromTimeSpan(dateTimeOffset.TimeOfDay), dateTimeOffset.Offset);
            }

            var value = new DateTime2ValueConverter.DateTime2
            {
                Date = dateTimeOffset,
                TimeZone = valueAsJsonObject["timeZone"]?.GetValue<string>(),
            };

            var jsonStr = _jsonSerializer.Serialize(value);
            return jsonStr;
        }

        /// <inheritdoc/>
        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);

            DateTime2ValueConverter.DateTime2? interValue = DateTime2ValueConverter.GetIntermediateFromSource(value, _jsonSerializer);
            if (interValue is null)
            {
                return null;
            }

            DateTime2Configuration? configuration = GetConfiguration(property.PropertyType.DataTypeKey);
            var objectValue = DateTime2ValueConverter.GetObjectFromIntermediate(interValue, configuration);

            JsonNode node = new JsonObject();
            node["date"] = objectValue is null ? null : $"{objectValue:O}";
            node["timeZone"] = interValue.TimeZone;
            return node;
        }

        private DateTime2Configuration? GetConfiguration(Guid dataTypeKey) =>
            _dataTypeConfigurationCache.GetConfigurationAs<DateTime2Configuration>(dataTypeKey);

        /// <summary>
        /// Validates the color selection for the color picker property editor.
        /// </summary>
        private class DateTime2Validator : IValueValidator
        {
            private readonly ILocalizedTextService _localizedTextService;

            public DateTime2Validator(ILocalizedTextService localizedTextService) => _localizedTextService = localizedTextService;

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
                var dataTypeConfig = dataTypeConfiguration as DateTime2Configuration;
                if (dataTypeConfig?.TimeZones?.Mode is not { } mode || mode == DateTime2Configuration.TimeZoneMode.None)
                {
                    yield break;
                }

                if (mode == DateTime2Configuration.TimeZoneMode.Custom
                    && dataTypeConfig.TimeZones.TimeZones.Any(t => t.Equals(selectedTimeZone, StringComparison.InvariantCultureIgnoreCase)) != true)
                {
                    yield return new ValidationResult(
                        _localizedTextService.Localize("validation", "notOneOfOptions", [selectedTimeZone]),
                        ["value"]);
                }
            }
        }
    }
}
