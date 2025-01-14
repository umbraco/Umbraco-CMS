// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a markdown editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MarkdownEditor,
    ValueType = ValueTypes.Text,
    ValueEditorIsReusable = true)]
public class MarkdownPropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MarkdownPropertyEditor" /> class.
    /// </summary>
    public MarkdownPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <summary>
    ///     Create a custom value editor
    /// </summary>
    /// <returns></returns>
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MarkDownPropertyValueEditor>(Attribute!);
}
