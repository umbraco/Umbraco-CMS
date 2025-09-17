// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.TimeOnly,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class TimeOnlyPropertyEditor : DateTime2PropertyEditorBase
{
    public TimeOnlyPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        ITimeOnlyPropertyIndexValueFactory propertyIndexValueFactory)
        : base(dataValueEditorFactory, ioHelper, propertyIndexValueFactory)
    {
    }

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<DateTime2DataValueEditor<TimeOnlyValueConverter>>(Attribute!);
}
