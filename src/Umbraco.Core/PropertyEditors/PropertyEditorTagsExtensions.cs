namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides extension methods for the <see cref="PropertyEditor"/> class to manage tags.
    /// </summary>
    public static class PropertyEditorTagsExtensions
    {
        /// <summary>
        /// Determines whether an editor supports tags.
        /// </summary>
        public static bool IsTagsEditor(this PropertyEditor editor)
            => editor?.GetType().GetCustomAttribute<TagsPropertyEditorAttribute>(false) != null;

        /// <summary>
        /// Gets the tags configuration attribute of an editor.
        /// </summary>
        public static TagsPropertyEditorAttribute GetTagAttribute(this PropertyEditor editor)
            => editor?.GetType().GetCustomAttribute<TagsPropertyEditorAttribute>(false);
    }
}
