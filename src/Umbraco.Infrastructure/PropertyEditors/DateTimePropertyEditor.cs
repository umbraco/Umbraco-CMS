// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data.SqlTypes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

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

    internal sealed class DateTimePropertyValueEditor : DataValueEditor
    {
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
            if (editorValue.DataTypeConfiguration is Dictionary<string, object> dictionary)
            {
                KeyValuePair<string, object> keyValuePair = dictionary.FirstOrDefault(kvp => kvp.Key is "Format");
                string value = keyValuePair.Value as string ?? string.Empty;

                if (!IsTimeOnly(value))
                {
                    return base.FromEditor(editorValue, currentValue);
                }

                if ((editorValue.Value is string stringValue && DateTime.TryParse(stringValue, out DateTime myDate))
                    || (editorValue.Value is DateTime dateValue && (myDate = dateValue) == dateValue))
                {
                    int year = myDate.Year > SqlDateTime.MinValue.Value.Year ? myDate.Year : SqlDateTime.MinValue.Value.Year;
                    return new DateTime(year, myDate.Month, myDate.Day, myDate.Hour, myDate.Minute, myDate.Second).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            return base.FromEditor(editorValue, currentValue);
        }

        private bool IsTimeOnly(string value)
        {
            HashSet<char> allowedSpecifiers = ['H', 'h', 'm', 's', 'f'];
            HashSet<char> allowedSeperators = ['/', '\\', '%', '-', ':', ' ', '.', '\'', '"'];

            foreach (char c in value)
            {
                if (allowedSpecifiers.Contains(c) || allowedSeperators.Contains(c))
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
