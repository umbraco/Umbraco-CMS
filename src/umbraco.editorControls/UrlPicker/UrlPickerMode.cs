namespace umbraco.editorControls.UrlPicker
{
    /// <summary>
    /// The modes this datatype can implement - they refer to how the local/external content is referred.
    /// </summary>
    public enum UrlPickerMode : int
    {
        /// <summary>
        /// URL string
        /// </summary>
        URL = 1,

        /// <summary>
        /// Content node
        /// </summary>
        Content = 2,

        /// <summary>
        /// Media node
        /// </summary>
        Media = 3,

        /// <summary>
        /// Upload a file
        /// </summary>
        Upload = 4
    }
}
