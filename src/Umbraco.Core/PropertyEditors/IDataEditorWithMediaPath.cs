namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Must be implemented by property editors that store media and return media paths
    /// </summary>
    /// <remarks>
    /// Currently there are only 2x core editors that do this: upload and image cropper.
    /// It would be possible for developers to know implement their own media property editors whereas previously this was not possible.
    /// </remarks>
    public interface IDataEditorWithMediaPath
    {
        /// <summary>
        /// Returns the media path for the value stored for a property
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string GetMediaPath(object value);
    }
}
