using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Media;
using Umbraco.Core.Media.Exif;
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
            ContentConfig = Current.Container.GetInstance<IContentSection>();
            Logger = Current.Container.GetInstance<ILogger>();
            MediaPathScheme = Current.Container.GetInstance<IMediaPathScheme>();
            MediaPathScheme.Initialize(this);

            UploadAutoFillProperties = new UploadAutoFillProperties(this, Logger, ContentConfig);
        }

        private IMediaPathScheme MediaPathScheme { get; }

        private IContentSection ContentConfig { get; }

        private ILogger Logger { get; }

        internal UploadAutoFillProperties UploadAutoFillProperties { get; }

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

        // gets or creates a property for a content item.
        private static Property GetProperty(IContentBase content, string propertyTypeAlias)
        {
            var property = content.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            if (property != null) return property;

            var propertyType = content.GetContentType().CompositionPropertyTypes
                .FirstOrDefault(x => x.Alias.InvariantEquals(propertyTypeAlias));
            if (propertyType == null)
                throw new Exception("No property type exists with alias " + propertyTypeAlias + ".");

            property = new Property(propertyType);
            content.Properties.Add(property);
            return property;
        }

        // fixme - what's below belongs to the upload property editor, not the media filesystem!

        public void SetUploadFile(IContentBase content, string propertyTypeAlias, string filename, Stream filestream, string culture = null, string segment = null)
        {
            var property = GetProperty(content, propertyTypeAlias);
            var oldpath = property.GetValue(culture, segment) is string svalue ? GetRelativePath(svalue) : null;
            var filepath = StoreFile(content, property.PropertyType, filename, filestream, oldpath);
            property.SetValue(GetUrl(filepath), culture, segment);
            SetUploadFile(content, property, filepath, filestream, culture, segment);
        }

        public void SetUploadFile(IContentBase content, string propertyTypeAlias, string filepath, string culture = null, string segment = null)
        {
            var property = GetProperty(content, propertyTypeAlias);
            // fixme delete?
            var oldpath = property.GetValue(culture, segment) is string svalue ? GetRelativePath(svalue) : null;
            if (string.IsNullOrWhiteSpace(oldpath) == false && oldpath != filepath)
                DeleteFile(oldpath);
            property.SetValue(GetUrl(filepath), culture, segment);
            using (var filestream = OpenFile(filepath))
            {
                SetUploadFile(content, property, filepath, filestream, culture, segment);
            }
        }

        // sets a file for the FileUpload property editor
        // ie generates thumbnails and populates autofill properties
        private void SetUploadFile(IContentBase content, Property property, string filepath, Stream filestream, string culture = null, string segment = null)
        {
            // will use filepath for extension, and filestream for length
            UploadAutoFillProperties.Populate(content, property.Alias, filepath, filestream, culture, segment);
        }

        #endregion

        #region Image

        /// <summary>
        /// Gets a value indicating whether the file extension corresponds to an image.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>A value indicating whether the file extension corresponds to an image.</returns>
        public bool IsImageFile(string extension)
        {
            if (extension == null) return false;
            extension = extension.TrimStart('.');
            return ContentConfig.ImageFileTypes.InvariantContains(extension);
        }

        /// <summary>
        /// Gets the dimensions of an image.
        /// </summary>
        /// <param name="stream">A stream containing the image bytes.</param>
        /// <returns>The dimension of the image.</returns>
        /// <remarks>First try with EXIF as it is faster and does not load the entire image
        /// in memory. Fallback to GDI which means loading the image in memory and thus
        /// use potentially large amounts of memory.</remarks>
        public Size GetDimensions(Stream stream)
        {
            //Try to load with exif
            try
            {
                var jpgInfo = ImageFile.FromStream(stream);

                if (jpgInfo.Format != ImageFileFormat.Unknown
                    && jpgInfo.Properties.ContainsKey(ExifTag.PixelYDimension)
                    && jpgInfo.Properties.ContainsKey(ExifTag.PixelXDimension))
                {
                    var height = Convert.ToInt32(jpgInfo.Properties[ExifTag.PixelYDimension].Value);
                    var width = Convert.ToInt32(jpgInfo.Properties[ExifTag.PixelXDimension].Value);
                    if (height > 0 && width > 0)
                    {
                        return new Size(width, height);
                    }
                }
            }
            catch (Exception)
            {
                //We will just swallow, just means we can't read exif data, we don't want to log an error either
            }

            //we have no choice but to try to read in via GDI
            using (var image = Image.FromStream(stream))
            {

                var fileWidth = image.Width;
                var fileHeight = image.Height;
                return new Size(fileWidth, fileHeight);
            }
        }

        #endregion
    }
}
