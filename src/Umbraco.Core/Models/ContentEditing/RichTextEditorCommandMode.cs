namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the mode in which a rich text editor command operates.
/// </summary>
public enum RichTextEditorCommandMode
{
    /// <summary>
    ///     The command inserts content at the cursor position.
    /// </summary>
    Insert,

    /// <summary>
    ///     The command operates on the current selection.
    /// </summary>
    Selection,

    /// <summary>
    ///     The command operates on all content.
    /// </summary>
    All,
}
