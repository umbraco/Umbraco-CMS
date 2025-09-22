// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public abstract class DateTimePropertyEditorBase : DataEditor
{
    private readonly IIOHelper _ioHelper;
    private readonly IPropertyIndexValueFactory _propertyIndexValueFactory;

    protected DateTimePropertyEditorBase(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IPropertyIndexValueFactory propertyIndexValueFactory)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _propertyIndexValueFactory = propertyIndexValueFactory;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new DateTimeConfigurationEditor(_ioHelper);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<DateTimeDataValueEditor>(Attribute!, MapDateToEditorFormat);

    protected abstract string MapDateToEditorFormat(DateTimeValueConverterBase.DateTimeDto dateTimeDto);

    /// <inheritdoc/>
    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _propertyIndexValueFactory;

    internal class DateTimeDataValueEditor : DataValueEditor
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger<DateTimeDataValueEditor> _logger;
        private readonly Func<DateTimeValueConverterBase.DateTimeDto, string> _mappingFunc;
        private readonly string _editorAlias;

        public DateTimeDataValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService,
            ILogger<DateTimeDataValueEditor> logger,
            Func<DateTimeValueConverterBase.DateTimeDto, string> mappingFunc)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
            _logger = logger;
            _mappingFunc = mappingFunc;
            _editorAlias = attribute.Alias;
            var validators = new TypedJsonValidatorRunner<DateTimeEditorValue, DateTimeConfiguration>(
                jsonSerializer,
                new DateTimeValidator(localizedTextService));

            Validators.Add(validators);
        }

        /// <inheritdoc/>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            if (editorValue.Value is null ||
                _jsonSerializer.TryDeserialize(editorValue.Value, out DateTimeEditorValue? dateTimeEditorValue) is false)
            {
                return base.FromEditor(editorValue, currentValue);
            }

            var selectedDate = dateTimeEditorValue.Date;
            if (selectedDate.IsNullOrWhiteSpace()
                || DateTimeOffset.TryParse(selectedDate, null, DateTimeStyles.AssumeUniversal, out DateTimeOffset dateTimeOffset) is false)
            {
                return null;
            }

            if (_editorAlias == Constants.PropertyEditors.Aliases.TimeOnly)
            {
                // Clear the date part if the format is TimeOnly.
                // This is needed because `DateTimeOffset.TryParse` does not support `DateTimeStyles.NoCurrentDateDefault`.
                dateTimeOffset = new DateTimeOffset(DateOnly.MinValue, TimeOnly.FromTimeSpan(dateTimeOffset.TimeOfDay), dateTimeOffset.Offset);
            }

            var value = new DateTimeValueConverterBase.DateTimeDto
            {
                Date = dateTimeOffset,
                TimeZone = dateTimeEditorValue.TimeZone,
            };

            return _jsonSerializer.Serialize(value);
        }

        /// <inheritdoc/>
        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);

            DateTimeValueConverterBase.DateTimeDto? dateTimeDto = DateTimePropertyEditorHelper.TryParseToIntermediateValue(value, _jsonSerializer, _logger, out DateTimeValueConverterBase.DateTimeDto? dateTimeDtoObj)
                ? dateTimeDtoObj
                : null;

            if (dateTimeDto is null)
            {
                return null;
            }

            return new DateTimeEditorValue
            {
                Date = _mappingFunc(dateTimeDto),
                TimeZone = dateTimeDto.TimeZone,
            };
        }

        /// <summary>
        /// Validates the date time selection for the DateTime2 property editor.
        /// </summary>
        private class DateTimeValidator : ITypedJsonValidator<DateTimeEditorValue, DateTimeConfiguration>
        {
            private readonly ILocalizedTextService _localizedTextService;

            public DateTimeValidator(ILocalizedTextService localizedTextService)
                => _localizedTextService = localizedTextService;

            public IEnumerable<ValidationResult> Validate(
                DateTimeEditorValue? value,
                DateTimeConfiguration? configuration,
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
                        _localizedTextService.Localize("validation", "invalidDate"),
                        ["value"]);
                }

                if (configuration?.TimeZones?.Mode is not { } mode)
                {
                    yield break;
                }

                if (mode == DateTimeConfiguration.TimeZoneMode.Custom
                    && configuration.TimeZones?.TimeZones.Any(t => t.Equals(value.TimeZone, StringComparison.InvariantCultureIgnoreCase)) != true)
                {
                    yield return new ValidationResult(
                        _localizedTextService.Localize("validation", "notOneOfOptions", [value.TimeZone]),
                        ["value"]);
                }
            }
        }
    }
}
