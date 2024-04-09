// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.ColorPicker,
    ValueEditorIsReusable = true)]
public class ColorPickerPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    public ColorPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        SupportsReadOnly = true;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory { get; } = new NoopPropertyIndexValueFactory();


    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ColorPickerConfigurationEditor(_ioHelper, _configurationEditorJsonSerializer);
}
