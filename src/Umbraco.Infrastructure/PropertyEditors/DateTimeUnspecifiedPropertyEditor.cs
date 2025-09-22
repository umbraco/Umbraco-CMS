// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.DateTimeUnspecified,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class DateTimeUnspecifiedPropertyEditor : DateTimePropertyEditorBase
{
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
