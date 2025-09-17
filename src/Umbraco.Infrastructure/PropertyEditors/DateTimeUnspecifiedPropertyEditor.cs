// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.DateTimeUnspecified,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class DateTimeUnspecifiedPropertyEditor : DateTime2PropertyEditorBase
{
    public DateTimeUnspecifiedPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IDateTimeUnspecifiedPropertyIndexValueFactory propertyIndexValueFactory)
        : base(dataValueEditorFactory, ioHelper, propertyIndexValueFactory)
    {
    }

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<DateTime2DataValueEditor<DateTimeUnspecifiedValueConverter>>(Attribute!);
}
