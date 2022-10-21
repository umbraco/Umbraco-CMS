// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a list-view editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ListView,
    "List view",
    "listview",
    HideLabel = true,
    Group = Constants.PropertyEditors.Groups.Lists,
    Icon = Constants.Icons.ListView,
    ValueEditorIsReusable = true)]
public class ListViewPropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _iioHelper;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public ListViewPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper iioHelper)
        : this(dataValueEditorFactory, iioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public ListViewPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper iioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _iioHelper = iioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ListViewConfigurationEditor(_iioHelper, _editorConfigurationParser);
}
