// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.MultiUrlPicker,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class MultiUrlPickerPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    public MultiUrlPickerPropertyEditor(IIOHelper ioHelper, IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory { get; } = new NoopPropertyIndexValueFactory();

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MultiUrlPickerConfigurationEditor(_ioHelper);

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultiUrlPickerValueEditor>(Attribute!);
}
