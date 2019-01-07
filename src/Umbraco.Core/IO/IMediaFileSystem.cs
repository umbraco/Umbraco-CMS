using System;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.IO
{
    /// <summary>
    /// Provides methods allowing the manipulation of media files.
    /// </summary>
    public interface IMediaFileSystem : IFileSystem
    {
        /// <summary>
        /// Delete media files.
        /// </summary>
        /// <param name="files">Files to delete (filesystem-relative paths).</param>
        void DeleteMediaFiles(IEnumerable<string> files);

        /// <summary>
        /// Gets the file path of a media file.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <param name="cuid">The unique identifier of the content/media owning the file.</param>
        /// <param name="puid">The unique identifier of the property type owning the file.</param>
        /// <returns>The filesystem-relative path to the media file.</returns>
        /// <remarks>With the old media path scheme, this CREATES a new media path each time it is invoked.</remarks>
        string GetMediaPath(string filename, Guid cuid, Guid puid);

        /// <summary>
        /// Gets the file path of a media file.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <param name="prevpath">A previous file path.</param>
        /// <param name="cuid">The unique identifier of the content/media owning the file.</param>
        /// <param name="puid">The unique identifier of the property type owning the file.</param>
        /// <returns>The filesystem-relative path to the media file.</returns>
        /// <remarks>In the old, legacy, number-based scheme, we try to re-use the media folder
        /// specified by <paramref name="prevpath"/>. Else, we CREATE a new one. Each time we are invoked.</remarks>
        string GetMediaPath(string filename, string prevpath, Guid cuid, Guid puid);

        /// <summary>
        /// Stores a media file associated to a property of a content item.
        /// </summary>
        /// <param name="content">The content item owning the media file.</param>
        /// <param name="propertyType">The property type owning the media file.</param>
        /// <param name="filename">The media file name.</param>
        /// <param name="filestream">A stream containing the media bytes.</param>
        /// <param name="oldpath">An optional filesystem-relative filepath to the previous media file.</param>
        /// <returns>The filesystem-relative filepath to the media file.</returns>
        /// <remarks>
        /// <para>The file is considered "owned" by the content/propertyType.</para>
        /// <para>If an <paramref name="oldpath"/> is provided then that file (and associated thumbnails if any) is deleted
        /// before the new file is saved, and depending on the media path scheme, the folder may be reused for the new file.</para>
        /// </remarks>
        string StoreFile(IContentBase content, PropertyType propertyType, string filename, Stream filestream, string oldpath);

        /// <summary>
        /// Copies a media file as a new media file, associated to a property of a content item.
        /// </summary>
        /// <param name="content">The content item owning the copy of the media file.</param>
        /// <param name="propertyType">The property type owning the copy of the media file.</param>
        /// <param name="sourcepath">The filesystem-relative path to the source media file.</param>
        /// <returns>The filesystem-relative path to the copy of the media file.</returns>
        string CopyFile(IContentBase content, PropertyType propertyType, string sourcepath);
    }
}
