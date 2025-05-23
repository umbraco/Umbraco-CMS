// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.DropDownListFlexible,
    ValueEditorIsReusable = true)]
public class DropDownFlexiblePropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    public DropDownFlexiblePropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        SupportsReadOnly = true;
    }

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleValueEditor>(Attribute!);

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new DropDownFlexibleConfigurationEditor(_ioHelper, _configurationEditorJsonSerializer);
}
