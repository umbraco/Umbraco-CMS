using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Media
{
    /// <summary>
    /// Provides helper methods for managing medias.
    /// </summary>
    /// <remarks>Medias can be anything that can be uploaded via an upload
    /// property, including but not limited to, images. See ImageHelper for
    /// image-specific methods.</remarks>
    internal static class MediaHelper
    {
        private static long _folderCounter;
        private static bool _folderCounterInitialized;
        private static readonly object FolderCounterLock = new object();

        // fixme - should be a config option of some sort!
        //public static bool UseTheNewMediaPathScheme { get; set; }
        public const bool UseTheNewMediaPathScheme = false;

        public static MediaFileSystem FileSystem { get { return FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>(); } }

        #region Media Path

        /// <summary>
        /// Gets the file path of a media file.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <param name="cuid">The unique identifier of the content/media owning the file.</param>
        /// <param name="puid">The unique identifier of the property type owning the file.</param>
        /// <returns>The filesystem-relative path to the media file.</returns>
        /// <remarks>With the old media path scheme, this CREATES a new media path each time it is invoked.</remarks>
        public static string GetMediaPath(string filename, Guid cuid, Guid puid)
        {
            filename = Path.GetFileName(filename);
            if (filename == null) throw new ArgumentException("Cannot become a safe filename.", "filename");
            filename = IOHelper.SafeFileName(filename.ToLowerInvariant());

            string folder;
            if (UseTheNewMediaPathScheme == false)
            {
                // old scheme: filepath is "<int>/<filename>" OR "<int>-<filename>"
                // default media filesystem maps to "~/media/<filepath>"
                folder = GetNextFolder();
            }
            else
            {
                // new scheme: path is "<cuid>-<puid>/<filename>" OR "<cuid>-<puid>-<filename>"
                // default media filesystem maps to "~/media/<filepath>"
                // fixme - this assumes that the keys exists and won't change (even when creating a new content)
                // fixme - this is going to create looooong filepaths, any chance we can shorten them?
                folder = cuid.ToString("N") + "-" + puid.ToString("N");
            }

            var filepath = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories
                ? Path.Combine(folder, filename)
                : folder + "-" + filename;

            return filepath;
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
        public static string GetMediaPath(string filename, string prevpath, Guid cuid, Guid puid)
        {
            if (UseTheNewMediaPathScheme || string.IsNullOrWhiteSpace(prevpath))
                return GetMediaPath(filename, cuid, puid);

            filename = Path.GetFileName(filename);
            if (filename == null) throw new ArgumentException("Cannot become a safe filename.", "filename");
            filename = IOHelper.SafeFileName(filename.ToLowerInvariant());

            // old scheme, with a previous path
            // prevpath should be "<int>/<filename>" OR "<int>-<filename>"
            // and we want to reuse the "<int>" part, so try to find it

            var sep = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories ? "/" : "-";
            var pos = prevpath.IndexOf(sep, StringComparison.Ordinal);
            var s = pos > 0 ? prevpath.Substring(0, pos) : null;
            int ignored;

            var folder = (pos > 0 && int.TryParse(s, out ignored)) ? s : GetNextFolder();

            // ReSharper disable once AssignNullToNotNullAttribute
            var filepath = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories
                ? Path.Combine(folder, filename)
                : folder + "-" + filename;

            return filepath;
        }

        /// <summary>
        /// Gets the next media folder in the original number-based scheme.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Should be private, is internal for legacy FileHandlerData which is obsolete.</remarks>
        internal static string GetNextFolder()
        {
            lock (FolderCounterLock)
            {
                if (_folderCounterInitialized == false)
                {
                    _folderCounter = 1000; // seed - was not respected in MediaSubfolderCounter?
                    var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
                    var directories = fs.GetDirectories("");
                    foreach (var directory in directories)
                    {
                        long folderNumber;
                        if (long.TryParse(directory, out folderNumber) && folderNumber > _folderCounter)
                            _folderCounter = folderNumber;
                    }

                    _folderCounterInitialized = true;
                }
            }

            return Interlocked.Increment(ref _folderCounter).ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        /// <summary>
        /// Stores a media file.
        /// </summary>
        /// <param name="content">The content item owning the media file.</param>
        /// <param name="propertyType">The property type owning the media file.</param>
        /// <param name="filename">The media file name.</param>
        /// <param name="filestream">A stream containing the media bytes.</param>
        /// <param name="oldpath">An optional filesystem-relative filepath to the previous media file.</param>
        /// <returns>The filesystem-relative filepath to the media file.</returns>
        /// <remarks>
        /// <para>The file is considered "owned" by the content/propertyType.</para>
        /// <para>If an <paramref name="oldpath"/> is provided then that file (and thumbnails) is deleted
        /// before the new file is saved, and depending on the media path scheme, the folder
        /// may be reused for the new file.</para>
        /// </remarks>
        public static string StoreFile(IContentBase content, PropertyType propertyType, string filename, Stream filestream, string oldpath)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (propertyType == null) throw new ArgumentNullException("propertyType");
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentException("Null or empty.", "filename");
            if (filestream == null) throw new ArgumentNullException("filestream");

            // clear the old file, if any
            var fs = FileSystem;
            if (string.IsNullOrWhiteSpace(oldpath) == false)
                ImageHelper.DeleteFile(fs, oldpath, true);

            // get the filepath, store the data
            // use oldpath as "prevpath" to try and reuse the folder, in original number-based scheme
            var filepath = GetMediaPath(filename, oldpath, content.Key, propertyType.Key);
            fs.AddFile(filepath, filestream);
            return filepath;
        }

        /// <summary>
        /// Clears a media file.
        /// </summary>
        /// <param name="filepath">The filesystem-relative path to the media file.</param>
        public static void DeleteFile(string filepath)
        {
            ImageHelper.DeleteFile(FileSystem, filepath, true);
        }

        /// <summary>
        /// Copies a media file.
        /// </summary>
        /// <param name="content">The content item owning the copy of the media file.</param>
        /// <param name="propertyType">The property type owning the copy of the media file.</param>
        /// <param name="sourcepath">The filesystem-relative path to the source media file.</param>
        /// <returns>The filesystem-relative path to the copy of the media file.</returns>
        public static string CopyFile(IContentBase content, PropertyType propertyType, string sourcepath)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (propertyType == null) throw new ArgumentNullException("propertyType");
            if (string.IsNullOrWhiteSpace(sourcepath)) throw new ArgumentException("Null or empty.", "sourcepath");

            // ensure we have a file to copy
            var fs = FileSystem;
            if (fs.FileExists(sourcepath) == false) return null;

            // get the filepath
            var filename = Path.GetFileName(sourcepath);
            var filepath = GetMediaPath(filename, content.Key, propertyType.Key);
            fs.CopyFile(sourcepath, filepath);
            ImageHelper.CopyThumbnails(fs, sourcepath, filepath);
            return filepath;
        }

        /// <summary>
        /// Gets or creates a property for a content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property type alias.</param>
        /// <returns>The property.</returns>
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

        public static void SetUploadFile(IContentBase content, string propertyTypeAlias, string filename, Stream filestream)
        {
            var property = GetProperty(content, propertyTypeAlias);
            var svalue = property.Value as string;
            var oldpath = svalue == null ? null : FileSystem.GetRelativePath(svalue);
            var filepath = StoreFile(content, property.PropertyType, filename, filestream, oldpath);
            property.Value = FileSystem.GetUrl(filepath);
            SetUploadFile(content, property, FileSystem, filepath, filestream);
        }

        public static void SetUploadFile(IContentBase content, string propertyTypeAlias, string filepath)
        {
            var property = GetProperty(content, propertyTypeAlias);
            var svalue = property.Value as string;
            var oldpath = svalue == null ? null : FileSystem.GetRelativePath(svalue); // FIXME DELETE?
            if (string.IsNullOrWhiteSpace(oldpath) == false && oldpath != filepath)
                FileSystem.DeleteFile(oldpath);
            property.Value = FileSystem.GetUrl(filepath);
            var fs = FileSystem;
            using (var filestream = fs.OpenFile(filepath))
            {
                SetUploadFile(content, property, fs, filepath, filestream);
            }
        }

        // sets a file for the FileUpload property editor
        // ie generates thumbnails and populates autofill properties
        private static void SetUploadFile(IContentBase content, Property property, IFileSystem fs, string filepath, Stream filestream)
        {
            // check if file is an image (and supports resizing and thumbnails etc)
            var extension = Path.GetExtension(filepath);
            var isImage = ImageHelper.IsImageFile(extension);

            // specific stuff for images (thumbnails etc)
            if (isImage)
            {
                using (var image = Image.FromStream(filestream))
                {
                    // use one image for all
                    ImageHelper.GenerateThumbnails(fs, image, filepath, property.PropertyType);
                    UploadAutoFillProperties.Populate(content, property.Alias, filepath, filestream, image);
                }
            }
            else
            {
                // will use filepath for extension, and filestream for length
                UploadAutoFillProperties.Populate(content, property.Alias, filepath, filestream);
            }
        }
    }
}
