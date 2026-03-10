// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Globalization;
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

/// <summary>
/// Provides base functionality for date time property editors that store their value as a JSON string with timezone information.
/// </summary>
public abstract class DateTimePropertyEditorBase : DataEditor
{
    private readonly IIOHelper _ioHelper;
    private readonly IPropertyIndexValueFactory _propertyIndexValueFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimePropertyEditorBase"/> class.
    /// </summary>
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

    /// <summary>
    /// Converts the specified date and time value to a string formatted for use in the property editor.
    /// </summary>
    /// <param name="dateTimeDto">An object containing the date and time components to be formatted.</param>
    /// <returns>A string representation of the date and time, formatted for use in the property editor.</returns>
    protected abstract string MapDateToEditorFormat(DateTimeValueConverterBase.DateTimeDto dateTimeDto);

    /// <inheritdoc/>
    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _propertyIndexValueFactory;

    /// <summary>
    /// Provides a data value editor for date and time properties, supporting conversion between editor values and
    /// persisted values for date/time property editors.
    /// </summary>
    internal class DateTimeDataValueEditor : DataValueEditor, IDataValueSortable
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger<DateTimeDataValueEditor> _logger;
        private readonly Func<DateTimeValueConverterBase.DateTimeDto, string> _mappingFunc;
        private readonly string _editorAlias;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeDataValueEditor"/> class.
        /// </summary>
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

        /// <inheritdoc />
        public string? GetSortableValue(object? value, object? dataTypeConfiguration)
        {
            if (value is not string jsonValue || string.IsNullOrWhiteSpace(jsonValue))
            {
                return null;
            }

            if (DateTimePropertyEditorHelper.TryParseToIntermediateValue(jsonValue, _jsonSerializer, _logger, out DateTimeValueConverterBase.DateTimeDto? dto) is false || dto is null)
            {
                return null;
            }

            // UTC-normalized ISO 8601 format for consistent lexicographic sorting
            // The "O" format specifier produces: yyyy-MM-ddTHH:mm:ss.fffffffZ
            return dto.Date.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
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
