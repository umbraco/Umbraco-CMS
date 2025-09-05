// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data.SqlTypes;
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
        return DataValueEditorFactory.Create<DateTimePropertyValueEditor>(Attribute!);

        // IDataValueEditor editor = base.CreateValueEditor();
        // editor.Validators.Add(new DateTimeValidator());
        // return editor;
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
                string[] array = ["hh:mm", "hh:mm:ss"];

                // Ensures we don't care about the customers formatting, weather the wish to write
                // hh:mm or HH:MM etc...
                if (!dictionary.Values.OfType<string>().Any(value => array.Contains(value, StringComparer.InvariantCultureIgnoreCase)))
                {
                    return editorValue.Value;
                }

                if (DateTime.TryParse(editorValue.Value as string, out DateTime myDate) || editorValue.Value is DateTime)
                {
                    editorValue.Value = new DateTime(SqlDateTime.MinValue.Value.Year, myDate.Month, myDate.Day, myDate.Hour, myDate.Minute, myDate.Second).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            return editorValue.Value;
        }
    }
}
