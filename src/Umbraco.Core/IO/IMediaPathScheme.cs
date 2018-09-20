using System;

namespace Umbraco.Core.IO
{
    /// <summary>
    /// Represents a media file path scheme.
    /// </summary>
    public interface IMediaPathScheme
    {
        // fixme
        // to anyone finding this code: YES the Initialize() method is CompletelyBroken™ (temporal whatever)
        // but at the moment, the media filesystem wants a scheme which wants a filesystem, and it's all
        // convoluted due to how filesystems are managed in FileSystems - clear that part first!

        /// <summary>
        /// Initialize.
        /// </summary>
        void Initialize(IFileSystem filesystem);

        /// <summary>
        /// Gets a media file path.
        /// </summary>
        /// <param name="itemGuid">The (content, media) item unique identifier.</param>
        /// <param name="propertyGuid">The property type unique identifier.</param>
        /// <param name="filename">The file name.</param>
        /// <param name="previous">A previous filename.</param>
        /// <returns>The filesystem-relative complete file path.</returns>
        string GetFilePath(Guid itemGuid, Guid propertyGuid, string filename, string previous = null);

        /// <summary>
        /// Gets the directory that can be deleted when the file is deleted.
        /// </summary>
        /// <param name="filepath">The filesystem-relative path of the file.</param>
        /// <returns>The filesystem-relative path of the directory.</returns>
        /// <remarks>
        /// <para>The directory, and anything below it, will be deleted.</para>
        /// <para>Can return null (or empty) when no directory should be deleted.</para>
        /// </remarks>
        string GetDeleteDirectory(string filepath);
    }
}
