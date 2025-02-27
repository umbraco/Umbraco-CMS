// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A property editor to allow the individual selection of pre-defined items.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.RadioButtonList,
    ValueType = ValueTypes.String,
    ValueEditorIsReusable = true)]
public class RadioButtonsPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    /// <summary>
    ///     The constructor will setup the property editor based on the attribute if one is found
    /// </summary>
    public RadioButtonsPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        SupportsReadOnly = true;
    }

    /// <summary>
    ///     Return a custom pre-value editor
    /// </summary>
    /// <returns></returns>
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ValueListConfigurationEditor(_ioHelper, _configurationEditorJsonSerializer);

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<RadioValueEditor>(Attribute!);
}
