// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a property editor for editing date-only values.
/// </summary>
/// <remarks>
/// This is one of four property editors derived from <see cref="DateTimePropertyEditorBase"/> and storing their value as JSON with timezone information.
/// </remarks>
[DataEditor(
    Constants.PropertyEditors.Aliases.DateOnly,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public sealed class DateOnlyPropertyEditor : DateTimePropertyEditorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateOnlyPropertyEditor"/> class.
    /// </summary>
    public DateOnlyPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IDateOnlyPropertyIndexValueFactory propertyIndexValueFactory)
        : base(dataValueEditorFactory, ioHelper, propertyIndexValueFactory)
    {
    }

    /// <inheritdoc />
    protected override string MapDateToEditorFormat(DateTimeValueConverterBase.DateTimeDto dateTimeDto)
        => dateTimeDto.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
