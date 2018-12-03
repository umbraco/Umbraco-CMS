using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LightInject;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Media;

using Umbraco.Core.Models;

namespace Umbraco.Core.IO
{
    /// <summary>
    /// A custom file system provider for media
    /// </summary>
    [FileSystemProvider("media")]
    public class MediaFileSystem : FileSystemWrapper
    {
        public MediaFileSystem(IFileSystem wrapped)
            : base(wrapped)
        {
            // due to how FileSystems is written at the moment, the ctor cannot be used to inject
            // dependencies, so we have to rely on property injection for anything we might need
            Current.Container.InjectProperties(this);
            MediaPathScheme.Initialize(this);
        }

        [Inject]
        internal IMediaPathScheme MediaPathScheme { get; set; }

        [Inject]
        internal IContentSection ContentConfig { get; set; }

        [Inject]
        internal ILogger Logger { get; set; }
        
        /// <summary>
        /// Deletes all files passed in.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        internal bool DeleteFiles(IEnumerable<string> files, Action<string, Exception> onError = null)
        {
            //ensure duplicates are removed
            files = files.Distinct();

            var allsuccess = true;
            var rootRelativePath = GetRelativePath("/");

            Parallel.ForEach(files, file =>
            {
                try
                {
                    if (file.IsNullOrWhiteSpace()) return;

                    var relativeFilePath = GetRelativePath(file);
                    if (FileExists(relativeFilePath) == false) return;

                    var parentDirectory = Path.GetDirectoryName(relativeFilePath);

                    // don't want to delete the media folder if not using directories.
                    if (ContentConfig.UploadAllowDirectories && parentDirectory != rootRelativePath)
                    {
                        //issue U4-771: if there is a parent directory the recursive parameter should be true
                        DeleteDirectory(parentDirectory, string.IsNullOrEmpty(parentDirectory) == false);
                    }
                    else
                    {
                        DeleteFile(file);
                    }
                }
                catch (Exception e)
                {
                    onError?.Invoke(file, e);
                    allsuccess = false;
                }
            });

            return allsuccess;
        }

        public void DeleteMediaFiles(IEnumerable<string> files)
        {
            files = files.Distinct();

            Parallel.ForEach(files, file =>
            {
                try
                {
                    if (file.IsNullOrWhiteSpace()) return;
                    if (FileExists(file) == false) return;
                    DeleteFile(file);

                    var directory = MediaPathScheme.GetDeleteDirectory(file);
                    if (!directory.IsNullOrWhiteSpace())
                        DeleteDirectory(directory, true);
                }
                catch (Exception e)
                {
                    Logger.Error<MediaFileSystem>(e, "Failed to delete attached file '{File}'", file);
                }
            });
        }

        #region Media Path

        /// <summary>
        /// Gets the file path of a media file.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <param name="cuid">The unique identifier of the content/media owning the file.</param>
        /// <param name="puid">The unique identifier of the property type owning the file.</param>
        /// <returns>The filesystem-relative path to the media file.</returns>
        /// <remarks>With the old media path scheme, this CREATES a new media path each time it is invoked.</remarks>
        public string GetMediaPath(string filename, Guid cuid, Guid puid)
        {
            filename = Path.GetFileName(filename);
            if (filename == null) throw new ArgumentException("Cannot become a safe filename.", nameof(filename));
            filename = IOHelper.SafeFileName(filename.ToLowerInvariant());

            return MediaPathScheme.GetFilePath(cuid, puid, filename);
        }

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
        public string GetMediaPath(string filename, string prevpath, Guid cuid, Guid puid)
        {
            filename = Path.GetFileName(filename);
            if (filename == null) throw new ArgumentException("Cannot become a safe filename.", nameof(filename));
            filename = IOHelper.SafeFileName(filename.ToLowerInvariant());

            return MediaPathScheme.GetFilePath(cuid, puid, filename, prevpath);
        }

        #endregion

        #region Associated Media Files

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
        public string StoreFile(IContentBase content, PropertyType propertyType, string filename, Stream filestream, string oldpath)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (propertyType == null) throw new ArgumentNullException(nameof(propertyType));
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentNullOrEmptyException(nameof(filename));
            if (filestream == null) throw new ArgumentNullException(nameof(filestream));

            // clear the old file, if any
            if (string.IsNullOrWhiteSpace(oldpath) == false)
                DeleteFile(oldpath);

            // get the filepath, store the data
            // use oldpath as "prevpath" to try and reuse the folder, in original number-based scheme
            var filepath = GetMediaPath(filename, oldpath, content.Key, propertyType.Key);
            AddFile(filepath, filestream);
            return filepath;
        }

        /// <summary>
        /// Copies a media file as a new media file, associated to a property of a content item.
        /// </summary>
        /// <param name="content">The content item owning the copy of the media file.</param>
        /// <param name="propertyType">The property type owning the copy of the media file.</param>
        /// <param name="sourcepath">The filesystem-relative path to the source media file.</param>
        /// <returns>The filesystem-relative path to the copy of the media file.</returns>
        public string CopyFile(IContentBase content, PropertyType propertyType, string sourcepath)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (propertyType == null) throw new ArgumentNullException(nameof(propertyType));
            if (string.IsNullOrWhiteSpace(sourcepath)) throw new ArgumentNullOrEmptyException(nameof(sourcepath));

            // ensure we have a file to copy
            if (FileExists(sourcepath) == false) return null;

            // get the filepath
            var filename = Path.GetFileName(sourcepath);
            var filepath = GetMediaPath(filename, content.Key, propertyType.Key);
            this.CopyFile(sourcepath, filepath);
            return filepath;
        }

        

        #endregion
        
    }
}
