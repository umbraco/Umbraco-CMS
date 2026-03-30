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

    /// <summary>
    /// Initializes a new instance of the <see cref="ListViewPropertyEditor"/> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">A factory for creating data value editors.</param>
    /// <param name="iioHelper">An <see cref="IIOHelper"/> instance used for file and directory operations.</param>
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
