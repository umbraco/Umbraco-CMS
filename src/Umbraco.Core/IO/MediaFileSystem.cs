using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Core.IO
{
    /// <summary>
    /// A custom file system provider for media
    /// </summary>
    public class MediaFileSystem : FileSystemWrapper, IMediaFileSystem
    {
        private readonly IMediaPathScheme _mediaPathScheme;
        private readonly IContentSection _contentConfig;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaFileSystem"/> class.
        /// </summary>
        public MediaFileSystem(IFileSystem innerFileSystem, IContentSection contentConfig, IMediaPathScheme mediaPathScheme, ILogger logger)
            : base(innerFileSystem)
        {
            _contentConfig = contentConfig;
            _mediaPathScheme = mediaPathScheme;
            _logger = logger;
        }

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

                    var directory = _mediaPathScheme.GetDeleteDirectory(this, file);
                    if (!directory.IsNullOrWhiteSpace())
                        DeleteDirectory(directory, true);
                }
                catch (Exception e)
                {
                    _logger.Error<MediaFileSystem,string>(e, "Failed to delete media file '{File}'.", file);
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

            return _mediaPathScheme.GetFilePath(this, cuid, puid, filename);
        }

        /// <inheritoc />
        public string GetMediaPath(string filename, string prevpath, Guid cuid, Guid puid)
        {
            filename = Path.GetFileName(filename);
            if (filename == null) throw new ArgumentException("Cannot become a safe filename.", nameof(filename));
            filename = IOHelper.SafeFileName(filename.ToLowerInvariant());

            return _mediaPathScheme.GetFilePath(this, cuid, puid, filename, prevpath);
        }

        #endregion

        #region Associated Media Files

        /// <inheritoc />
        public string StoreFile(IContentBase content, PropertyType propertyType, string filename, Stream filestream, string oldpath)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (propertyType == null) throw new ArgumentNullException(nameof(propertyType));
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(filename));
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
            if (sourcepath == null) throw new ArgumentNullException(nameof(sourcepath));
            if (string.IsNullOrWhiteSpace(sourcepath)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(sourcepath));

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
