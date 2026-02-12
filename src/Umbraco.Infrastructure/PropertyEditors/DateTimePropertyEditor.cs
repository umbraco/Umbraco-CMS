// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a date and time property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.DateTime,
    ValueType = ValueTypes.DateTime,
    ValueEditorIsReusable = true)]
public class DateTimePropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DateTimePropertyEditor" /> class.
    /// </summary>
    public DateTimePropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
    {
        DateTimePropertyValueEditor editor = DataValueEditorFactory.Create<DateTimePropertyValueEditor>(Attribute!);
        editor.Validators.Add(new DateTimeValidator());
        return editor;
    }

    /// <summary>
    ///    Provides a value editor for the datetime property editor.
    /// </summary>
    internal sealed class DateTimePropertyValueEditor : DataValueEditor
    {
        /// <summary>
        ///   The key used to retrieve the date format from the data type configuration.
        /// </summary>
        internal const string DateTypeConfigurationFormatKey = "format";

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePropertyValueEditor"/> class.
        /// </summary>
        public DateTimePropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
        }

        /// <inheritdoc/>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            if (editorValue.Value is null)
            {
                return base.FromEditor(editorValue, currentValue);
            }

            if (TryGetConfiguredDateTimeFormat(editorValue, out string? format) is false)
            {
                return base.FromEditor(editorValue, currentValue);
            }

            if (IsTimeOnlyFormat(format) is false)
            {
                return base.FromEditor(editorValue, currentValue);
            }

            // We have a time-only format, so we need to ensure the date part is valid for SQL Server, so we can persist
            // without error.
            // If we have a date part that is less than the minimum date, we need to adjust it to be the minimum date.
            Attempt<object?> dateConvertAttempt = editorValue.Value.TryConvertTo(typeof(DateTime?));
            if (dateConvertAttempt.Success is false || dateConvertAttempt.Result is null)
            {
                return base.FromEditor(editorValue, currentValue);
            }

            var dateTimeValue = (DateTime)dateConvertAttempt.Result;
            int yearValue = dateTimeValue.Year > SqlDateTime.MinValue.Value.Year
                ? dateTimeValue.Year
                : SqlDateTime.MinValue.Value.Year;
            return new DateTime(yearValue, dateTimeValue.Month, dateTimeValue.Day, dateTimeValue.Hour, dateTimeValue.Minute, dateTimeValue.Second);
        }

        private static bool TryGetConfiguredDateTimeFormat(ContentPropertyData editorValue, [NotNullWhen(true)] out string? format)
        {
            if (editorValue.DataTypeConfiguration is not Dictionary<string, object> dataTypeConfigurationDictionary)
            {
                format = null;
                return false;
            }

            KeyValuePair<string, object> keyValuePair = dataTypeConfigurationDictionary
                .FirstOrDefault(kvp => kvp.Key is "format");
            format = keyValuePair.Value as string;
            return string.IsNullOrWhiteSpace(format) is false;
        }

        private static bool IsTimeOnlyFormat(string format)
        {
            DateTime testDate = DateTime.UtcNow;
            var testDateFormatted = testDate.ToString(format);
            if (DateTime.TryParseExact(testDateFormatted, format, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out DateTime parsedDate) is false)
            {
                return false;
            }

            return parsedDate.Year == 1 && parsedDate.Month == 1 && parsedDate.Day == 1;
        }
    }
}
