// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a property editor for editing time-only values.
/// </summary>
/// <remarks>
/// This is one of four property editors derived from <see cref="DateTimePropertyEditorBase"/> and storing their value as JSON with timezone information.
/// </remarks>
[DataEditor(
    Constants.PropertyEditors.Aliases.TimeOnly,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class TimeOnlyPropertyEditor : DateTimePropertyEditorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeOnlyPropertyEditor"/> class.
    /// </summary>
    public TimeOnlyPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        ITimeOnlyPropertyIndexValueFactory propertyIndexValueFactory)
        : base(dataValueEditorFactory, ioHelper, propertyIndexValueFactory)
    {
    }

    /// <inheritdoc />
    protected override string MapDateToEditorFormat(DateTimeValueConverterBase.DateTimeDto dateTimeDto)
        => dateTimeDto.Date.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
}
