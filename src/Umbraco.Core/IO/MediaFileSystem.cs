using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Media;
using Umbraco.Core.Models;

namespace Umbraco.Core.IO
{
    /// <summary>
    /// A custom file system provider for media
    /// </summary>
    [FileSystem("media")]
    public class MediaFileSystem : FileSystemWrapper, IMediaFileSystem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaFileSystem"/> class.
        /// </summary>
        public MediaFileSystem(IFileSystem innerFileSystem)
            : base(innerFileSystem)
        {
            ContentConfig = Current.Container.GetInstance<IContentSection>();
            Logger = Current.Container.GetInstance<ILogger>();
            MediaPathScheme = Current.Container.GetInstance<IMediaPathScheme>();
            MediaPathScheme.Initialize(this);
        }

        private IMediaPathScheme MediaPathScheme { get; }

        private IContentSection ContentConfig { get; }

        private ILogger Logger { get; }

        /// <inheritoc />
        public void DeleteMediaFiles(IEnumerable<string> files)
        {
            files = files.Distinct();

            // kinda try to keep things under control
            var options = new ParallelOptions { MaxDegreeOfParallelism = 20 };

            Parallel.ForEach(files, options, file =>
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
                    Logger.Error<MediaFileSystem>(e, "Failed to delete media file '{File}'.", file);
                }
            });
        }

        #region Media Path

        /// <inheritoc />
        public string GetMediaPath(string filename, Guid cuid, Guid puid)
        {
            filename = Path.GetFileName(filename);
            if (filename == null) throw new ArgumentException("Cannot become a safe filename.", nameof(filename));
            filename = IOHelper.SafeFileName(filename.ToLowerInvariant());

            return MediaPathScheme.GetFilePath(cuid, puid, filename);
        }

        /// <inheritoc />
        public string GetMediaPath(string filename, string prevpath, Guid cuid, Guid puid)
        {
            filename = Path.GetFileName(filename);
            if (filename == null) throw new ArgumentException("Cannot become a safe filename.", nameof(filename));
            filename = IOHelper.SafeFileName(filename.ToLowerInvariant());

            return MediaPathScheme.GetFilePath(cuid, puid, filename, prevpath);
        }

        #endregion

        #region Associated Media Files

        /// <inheritoc />
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

        /// <inheritoc />
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
