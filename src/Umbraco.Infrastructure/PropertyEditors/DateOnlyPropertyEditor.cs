// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.DateOnly,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public sealed class DateOnlyPropertyEditor : DateTimePropertyEditorBase
{
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
