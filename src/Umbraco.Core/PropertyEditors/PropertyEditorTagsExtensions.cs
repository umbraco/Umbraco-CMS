using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="IDataEditor" /> interface to manage tags.
/// </summary>
public static class PropertyEditorTagsExtensions
{
    /// <summary>
    ///     Determines whether an editor supports tags.
    /// </summary>
    public static bool IsTagsEditor(this IDataEditor editor)
        => editor.GetTagAttribute() != null;

    /// <summary>
    ///     Gets the tags configuration attribute of an editor.
    /// </summary>
    public static TagsPropertyEditorAttribute? GetTagAttribute(this IDataEditor? editor)
        => editor?.GetType().GetCustomAttribute<TagsPropertyEditorAttribute>(false);
}
