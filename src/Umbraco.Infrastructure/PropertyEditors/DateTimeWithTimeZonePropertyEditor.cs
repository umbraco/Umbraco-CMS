// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a property editor for editing a date/time value with timezone.
/// </summary>
/// <remarks>
/// This is one of four property editors derived from <see cref="DateTimePropertyEditorBase"/> and storing their value as JSON with timezone information.
/// </remarks>
[DataEditor(
    Constants.PropertyEditors.Aliases.DateTimeWithTimeZone,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class DateTimeWithTimeZonePropertyEditor : DateTimePropertyEditorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeWithTimeZonePropertyEditor"/> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory used to create data value editors for property values.</param>
    /// <param name="ioHelper">Helper for IO operations, such as path resolution.</param>
    /// <param name="propertyIndexValueFactory">Factory for creating index values for date and time properties with time zone support.</param>
    public DateTimeWithTimeZonePropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IDateTimeWithTimeZonePropertyIndexValueFactory propertyIndexValueFactory)
        : base(dataValueEditorFactory, ioHelper, propertyIndexValueFactory)
    {
    }

    /// <inheritdoc />
    protected override string MapDateToEditorFormat(DateTimeValueConverterBase.DateTimeDto dateTimeDto)
        => dateTimeDto.Date.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
}
