// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A property editor to allow the individual selection of pre-defined items.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.RadioButtonList,
    "Radio button list",
    "radiobuttons",
    ValueType = ValueTypes.String,
    Group = Constants.PropertyEditors.Groups.Lists,
    Icon = "icon-target",
    ValueEditorIsReusable = true)]
public class RadioButtonsPropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;
    private readonly ILocalizedTextService _localizedTextService;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public RadioButtonsPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        ILocalizedTextService localizedTextService)
        : this(dataValueEditorFactory, ioHelper, localizedTextService, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     The constructor will setup the property editor based on the attribute if one is found
    /// </summary>
    public RadioButtonsPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        ILocalizedTextService localizedTextService,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _localizedTextService = localizedTextService;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    /// <summary>
    ///     Return a custom pre-value editor
    /// </summary>
    /// <returns></returns>
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ValueListConfigurationEditor(_localizedTextService, _ioHelper, _editorConfigurationParser);
}
