// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a list-view editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ListView,
    ValueEditorIsReusable = true)]
public class ListViewPropertyEditor : DataEditor
{
    private readonly IIOHelper _iioHelper;

    public ListViewPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper iioHelper)
        : base(dataValueEditorFactory)
    {
        _iioHelper = iioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ListViewConfigurationEditor(_iioHelper);
}
