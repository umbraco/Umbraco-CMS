// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A property editor to allow multiple checkbox selection of pre-defined items.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.CheckBoxList,
    "Checkbox list",
    "checkboxlist",
    Icon = "icon-bulleted-list",
    Group = Constants.PropertyEditors.Groups.Lists,
    ValueEditorIsReusable = true)]
public class CheckBoxListPropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;
    private readonly ILocalizedTextService _textService;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public CheckBoxListPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ILocalizedTextService textService,
        IIOHelper ioHelper)
        : this(dataValueEditorFactory, textService, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     The constructor will setup the property editor based on the attribute if one is found
    /// </summary>
    public CheckBoxListPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        ILocalizedTextService textService,
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _textService = textService;
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ValueListConfigurationEditor(_textService, _ioHelper, _editorConfigurationParser);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleValueEditor>(Attribute!);
}
