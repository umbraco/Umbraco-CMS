// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A property editor to allow multiple checkbox selection of pre-defined items.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.CheckBoxList,
    ValueEditorIsReusable = true)]
public class CheckBoxListPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    /// <summary>
    ///     The constructor will setup the property editor based on the attribute if one is found
    /// </summary>
    public CheckBoxListPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ValueListConfigurationEditor(_ioHelper, _configurationEditorJsonSerializer);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleValueEditor>(Attribute!);
}
