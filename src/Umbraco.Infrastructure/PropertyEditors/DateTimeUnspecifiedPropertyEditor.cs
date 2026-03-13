// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a property editor for editing an unspecified date/time value.
/// </summary>
/// <remarks>
/// This is one of four property editors derived from <see cref="DateTimePropertyEditorBase"/> and storing their value as JSON with timezone information.
/// </remarks>
[DataEditor(
    Constants.PropertyEditors.Aliases.DateTimeUnspecified,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class DateTimeUnspecifiedPropertyEditor : DateTimePropertyEditorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeUnspecifiedPropertyEditor"/> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory for creating data value editors for property values.</param>
    /// <param name="ioHelper">Helper for IO operations, such as path and file handling.</param>
    /// <param name="propertyIndexValueFactory">Factory for creating index values for date/time properties with unspecified kind.</param>
    public DateTimeUnspecifiedPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IDateTimeUnspecifiedPropertyIndexValueFactory propertyIndexValueFactory)
        : base(dataValueEditorFactory, ioHelper, propertyIndexValueFactory)
    {
    }

    /// <inheritdoc />
    protected override string MapDateToEditorFormat(DateTimeValueConverterBase.DateTimeDto dateTimeDto)
        => dateTimeDto.Date.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
}
