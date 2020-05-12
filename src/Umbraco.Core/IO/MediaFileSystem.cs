using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
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
        private readonly IContentSection _contentConfig;
        private readonly UploadAutoFillProperties _uploadAutoFillProperties;
        private readonly ILogger _logger;

        private readonly object _folderCounterLock = new object();
        private long _folderCounter;
        private bool _folderCounterInitialized;

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        private static readonly Dictionary<int, string> DefaultSizes = new Dictionary<int, string>
        {
            { 100, "thumb" },
            { 500, "big-thumb" }
        };

        public MediaFileSystem(IFileSystem wrapped)
            : this(wrapped, UmbracoConfig.For.UmbracoSettings().Content, ApplicationContext.Current.ProfilingLogger.Logger)
        { }

        public MediaFileSystem(IFileSystem wrapped, IContentSection contentConfig, ILogger logger)
            : base(wrapped)
        {
            _logger = logger;
            _contentConfig = contentConfig;
            _uploadAutoFillProperties = new UploadAutoFillProperties(this, logger, contentConfig);
        }

        internal UploadAutoFillProperties UploadAutoFillProperties { get { return _uploadAutoFillProperties; } }

        // note - this is currently experimental / being developed
        //public static bool UseTheNewMediaPathScheme { get; set; }
        public const bool UseTheNewMediaPathScheme = false;

        // none of the methods below are used in Core anymore

        [Obsolete("This low-level method should NOT exist.")]
        public string GetRelativePath(int propertyId, string fileName)
        {
            var sep = _contentConfig.UploadAllowDirectories
                ? Path.DirectorySeparatorChar
                : '-';

            return propertyId.ToString(CultureInfo.InvariantCulture) + sep + fileName;
        }

        [Obsolete("This low-level method should NOT exist.", false)]
        public string GetRelativePath(string subfolder, string fileName)
        {
            var sep = _contentConfig.UploadAllowDirectories
                ? Path.DirectorySeparatorChar
                : '-';

            return subfolder + sep + fileName;
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
                // new scheme: path is "<xuid>/<filename>" where xuid is a combination of cuid and puid
                // default media filesystem maps to "~/media/<filepath>"
                // assumes that cuid and puid keys can be trusted - and that a single property type
                // for a single content cannot store two different files with the same name
                folder = Combine(cuid, puid).ToHexString(/*'/', 2, 4*/); // could use ext to fragment path eg 12/e4/f2/...
            }

            var filepath = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories
                ? Path.Combine(folder, filename)
                : folder + "-" + filename;

            return filepath;
        }

        private static byte[] Combine(Guid guid1, Guid guid2)
        {
            var bytes1 = guid1.ToByteArray();
            var bytes2 = guid2.ToByteArray();
            var bytes = new byte[bytes1.Length];
            for (var i = 0; i < bytes1.Length; i++)
                bytes[i] = (byte)(bytes1[i] ^ bytes2[i]);
            return bytes;
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
                ? Path.Combine(folder, filename).Replace('\\', '/')
                : folder + "-" + filename;

            return filepath;
        }

        /// <summary>
        /// Gets the next media folder in the original number-based scheme.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Should be private, is internal for legacy FileHandlerData which is obsolete.</remarks>
        internal string GetNextFolder()
        {
            EnsureFolderCounterIsInitialized();
            return Interlocked.Increment(ref _folderCounter).ToString(CultureInfo.InvariantCulture);
        }

        private void EnsureFolderCounterIsInitialized()
        {
            lock (_folderCounterLock)
            {
                if (_folderCounterInitialized) return;

                _folderCounter = 1000; // seed
                var directories = GetDirectories("");
                foreach (var directory in directories)
                {
                    long folderNumber;
                    if (long.TryParse(directory, out folderNumber) && folderNumber > _folderCounter)
                        _folderCounter = folderNumber;
                }

                // note: not multi-domains ie LB safe as another domain could create directories
                // while we read and parse them - don't fix, move to new scheme eventually

                _folderCounterInitialized = true;
            }
        }

        #endregion

        #region Associated Media Files

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
        public string StoreFile(IContentBase content, PropertyType propertyType, string filename, Stream filestream, string oldpath)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (propertyType == null) throw new ArgumentNullException("propertyType");
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentException("Null or empty.", "filename");
            if (filestream == null) throw new ArgumentNullException("filestream");

            // clear the old file, if any
            if (string.IsNullOrWhiteSpace(oldpath) == false)
                DeleteFile(oldpath, true);

            // get the filepath, store the data
            // use oldpath as "prevpath" to try and reuse the folder, in original number-based scheme
            var filepath = GetMediaPath(filename, oldpath, content.Key, propertyType.Key);
            AddFile(filepath, filestream);
            return filepath;
        }

        /// <summary>
        /// Clears a media file.
        /// </summary>
        /// <param name="filepath">The filesystem-relative path to the media file.</param>
        public new void DeleteFile(string filepath)
        {
            DeleteFile(filepath, true);
        }

        /// <summary>
        /// Copies a media file.
        /// </summary>
        /// <param name="content">The content item owning the copy of the media file.</param>
        /// <param name="propertyType">The property type owning the copy of the media file.</param>
        /// <param name="sourcepath">The filesystem-relative path to the source media file.</param>
        /// <returns>The filesystem-relative path to the copy of the media file.</returns>
        public string CopyFile(IContentBase content, PropertyType propertyType, string sourcepath)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (propertyType == null) throw new ArgumentNullException("propertyType");
            if (string.IsNullOrWhiteSpace(sourcepath)) throw new ArgumentException("Null or empty.", "sourcepath");

            // ensure we have a file to copy
            if (FileExists(sourcepath) == false) return null;

            // get the filepath
            var filename = Path.GetFileName(sourcepath);
            var filepath = GetMediaPath(filename, content.Key, propertyType.Key);
            this.CopyFile(sourcepath, filepath);

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

        public void SetUploadFile(IContentBase content, string propertyTypeAlias, string filename, Stream filestream)
        {
            var property = GetProperty(content, propertyTypeAlias);
            var svalue = property.Value as string;
            if (svalue != null && svalue.DetectIsJson())
            {
                // the property value is a JSON serialized image crop data set - grab the "src" property as the file source
                var jObject = JsonConvert.DeserializeObject<JObject>(svalue);
                svalue = jObject != null ? jObject.GetValueAsString("src") : svalue;
            }
            var oldpath = svalue == null ? null : GetRelativePath(svalue);
            var filepath = StoreFile(content, property.PropertyType, filename, filestream, oldpath);
            property.Value = GetUrl(filepath);
            SetUploadFile(content, property, filepath, filestream);
        }

        public void SetUploadFile(IContentBase content, string propertyTypeAlias, string filepath)
        {
            var property = GetProperty(content, propertyTypeAlias);
            var svalue = property.Value as string;
            var oldpath = svalue == null ? null : GetRelativePath(svalue);
            if (string.IsNullOrWhiteSpace(oldpath) == false && oldpath != filepath)
                DeleteFile(oldpath, true);
            property.Value = GetUrl(filepath);
            using (var filestream = OpenFile(filepath))
            {
                SetUploadFile(content, property, filepath, filestream);
            }
        }

        /// <summary>
        /// Sets a file for the FileUpload property editor and populates autofill properties
        /// </summary>
        /// <param name="content"></param>
        /// <param name="property"></param>
        /// <param name="filepath"></param>
        /// <param name="filestream"></param>
        private void SetUploadFile(IContentBase content, Property property, string filepath, Stream filestream)
        {
            // will use filepath for extension, and filestream for length
            _uploadAutoFillProperties.Populate(content, property.Alias, filepath, filestream);
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
            return UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes.InvariantContains(extension);
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

                if (jpgInfo != null
                    && jpgInfo.Format != ImageFileFormat.Unknown
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
            catch
            {
                //We will just swallow, just means we can't read exif data, we don't want to log an error either
            }

            //we have no choice but to try to read in via GDI
            try
            {
                using (var image = Image.FromStream(stream))
                {
                    var fileWidth = image.Width;
                    var fileHeight = image.Height;
                    return new Size(fileWidth, fileHeight);
                }
            }
            catch
            {
                //We will just swallow, just means we can't read via GDI, we don't want to log an error either
            }

            return new Size(Constants.Conventions.Media.DefaultSize, Constants.Conventions.Media.DefaultSize);
        }

        #endregion

        #region Manage thumbnails

        // note: this does not find 'custom' thumbnails?
        // will find _thumb and _big-thumb but NOT _custom?
        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public IEnumerable<string> GetThumbnails(string path)
        {
            var parentDirectory = Path.GetDirectoryName(path);
            var extension = Path.GetExtension(path);

            return GetFiles(parentDirectory)
                .Where(x => x.StartsWith(path.TrimEnd(extension) + "_thumb") || x.StartsWith(path.TrimEnd(extension) + "_big-thumb"))
                .ToList();
        }

        public void DeleteFile(string path, bool deleteThumbnails)
        {
            base.DeleteFile(path);

            if (deleteThumbnails == false)
                return;

            DeleteThumbnails(path);
        }

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public void DeleteThumbnails(string path)
        {
            GetThumbnails(path)
                .ForEach(x => base.DeleteFile(x));
        }

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public void CopyThumbnails(string sourcePath, string targetPath)
        {
            var targetPathBase = Path.GetDirectoryName(targetPath) ?? "";
            foreach (var sourceThumbPath in GetThumbnails(sourcePath))
            {
                var sourceThumbFilename = Path.GetFileName(sourceThumbPath) ?? "";
                var targetThumbPath = Path.Combine(targetPathBase, sourceThumbFilename);
                this.CopyFile(sourceThumbPath, targetThumbPath);
            }
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
                    DeleteFile(file, true);

                    if (UseTheNewMediaPathScheme == false)
                    {
                        // old scheme: filepath is "<int>/<filename>" OR "<int>-<filename>"
                        // remove the directory if any
                        var dir = Path.GetDirectoryName(file);
                        if (string.IsNullOrWhiteSpace(dir) == false)
                            DeleteDirectory(dir, true);
                    }
                    else
                    {
                        // new scheme: path is "<xuid>/<filename>" where xuid is a combination of cuid and puid
                        // remove the directory
                        var dir = Path.GetDirectoryName(file);
                        DeleteDirectory(dir, true);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error<MediaFileSystem>("Failed to delete attached file \"" + file + "\".", e);
                }
            });
        }


        #endregion

        #region GenerateThumbnails

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public IEnumerable<ResizedImage> GenerateThumbnails(
            Image image,
            string filepath,
            string preValue)
        {
            if (string.IsNullOrWhiteSpace(preValue))
                return GenerateThumbnails(image, filepath);

            var additionalSizes = new List<int>();
            var sep = preValue.Contains(",") ? "," : ";";
            var values = preValue.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var value in values)
            {
                int size;
                if (int.TryParse(value, out size))
                    additionalSizes.Add(size);
            }

            return GenerateThumbnails(image, filepath, additionalSizes);
        }

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public IEnumerable<ResizedImage> GenerateThumbnails(
            Image image,
            string filepath,
            IEnumerable<int> additionalSizes = null)
        {
            var w = image.Width;
            var h = image.Height;

            var sizes = additionalSizes == null ? DefaultSizes.Keys : DefaultSizes.Keys.Concat(additionalSizes);

            // start with default sizes,
            // add additional sizes,
            // filter out duplicates,
            // filter out those that would be larger that the original image
            // and create the thumbnail
            return sizes
                .Distinct()
                .Where(x => w >= x && h >= x)
                .Select(x => GenerateResized(image, filepath, DefaultSizes.ContainsKey(x) ? DefaultSizes[x] : "", x))
                .ToList(); // now
        }

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public IEnumerable<ResizedImage> GenerateThumbnails(
            Stream filestream,
            string filepath,
            PropertyType propertyType)
        {
            // get the original image from the original stream
            if (filestream.CanSeek) filestream.Seek(0, 0);
            using (var image = Image.FromStream(filestream))
            {
                return GenerateThumbnails(image, filepath, propertyType);
            }
        }

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public IEnumerable<ResizedImage> GenerateThumbnails(
            Image image,
            string filepath,
            PropertyType propertyType)
        {
            // if the editor is an upload field, check for additional thumbnail sizes
            // that can be defined in the prevalue for the property data type. otherwise,
            // just use the default sizes.
            var sizes = propertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias
                ? ApplicationContext.Current.Services.DataTypeService
                    .GetPreValuesByDataTypeId(propertyType.DataTypeDefinitionId)
                    .FirstOrDefault()
                : string.Empty;

            return GenerateThumbnails(image, filepath, sizes);
        }

        #endregion

        #region GenerateResized - Generate at resized filepath derived from origin filepath

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public ResizedImage GenerateResized(Image originImage, string originFilepath, string sizeName, int maxWidthHeight)
        {
            return GenerateResized(originImage, originFilepath, sizeName, maxWidthHeight, -1, -1);
        }

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public ResizedImage GenerateResized(Image originImage, string originFilepath, string sizeName, int fixedWidth, int fixedHeight)
        {
            return GenerateResized(originImage, originFilepath, sizeName, -1, fixedWidth, fixedHeight);
        }

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public ResizedImage GenerateResized(Image originImage, string originFilepath, string sizeName, int maxWidthHeight, int fixedWidth, int fixedHeight)
        {
            if (string.IsNullOrWhiteSpace(sizeName))
                sizeName = "UMBRACOSYSTHUMBNAIL";
            var extension = Path.GetExtension(originFilepath) ?? string.Empty;
            var filebase = originFilepath.TrimEnd(extension);
            var resizedFilepath = filebase + "_" + sizeName + extension;

            return GenerateResizedAt(originImage, resizedFilepath, maxWidthHeight, fixedWidth, fixedHeight);
        }

        #endregion

        #region GenerateResizedAt - Generate at specified resized filepath

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public ResizedImage GenerateResizedAt(Image originImage, string resizedFilepath, int maxWidthHeight)
        {
            return GenerateResizedAt(originImage, resizedFilepath, maxWidthHeight, -1, -1);
        }

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public ResizedImage GenerateResizedAt(Image originImage, int fixedWidth, int fixedHeight, string resizedFilepath)
        {
            return GenerateResizedAt(originImage, resizedFilepath, -1, fixedWidth, fixedHeight);
        }

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public ResizedImage GenerateResizedAt(Image originImage, string resizedFilepath, int maxWidthHeight, int fixedWidth, int fixedHeight)
        {
            // target dimensions
            int width, height;

            // if maxWidthHeight then get ratio
            if (maxWidthHeight > 0)
            {
                var fx = (float)originImage.Size.Width / maxWidthHeight;
                var fy = (float)originImage.Size.Height / maxWidthHeight;
                var f = Math.Max(fx, fy); // fit in thumbnail size
                width = (int)Math.Round(originImage.Size.Width / f);
                height = (int)Math.Round(originImage.Size.Height / f);
                if (width == 0) width = 1;
                if (height == 0) height = 1;
            }
            else if (fixedWidth > 0 && fixedHeight > 0)
            {
                width = fixedWidth;
                height = fixedHeight;
            }
            else
            {
                width = height = 1;
            }

            // create new image with best quality settings
            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // if the image size is rather large we cannot use the best quality interpolation mode
                // because we'll get out of mem exceptions. So we detect how big the image is and use
                // the mid quality interpolation mode when the image size exceeds our max limit.
                graphics.InterpolationMode = originImage.Width > 5000 || originImage.Height > 5000
                    ? InterpolationMode.Bilinear // mid quality
                    : InterpolationMode.HighQualityBicubic; // best quality

                // everything else is best-quality
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                // copy the old image to the new and resize
                var rect = new Rectangle(0, 0, width, height);
                graphics.DrawImage(originImage, rect, 0, 0, originImage.Width, originImage.Height, GraphicsUnit.Pixel);

                // get an encoder - based upon the file type
                var extension = (Path.GetExtension(resizedFilepath) ?? "").TrimStart('.').ToLowerInvariant();
                var encoders = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo encoder;
                switch (extension)
                {
                    case "png":
                        encoder = encoders.Single(t => t.MimeType.Equals("image/png"));
                        break;
                    case "gif":
                        encoder = encoders.Single(t => t.MimeType.Equals("image/gif"));
                        break;
                    case "tif":
                    case "tiff":
                        encoder = encoders.Single(t => t.MimeType.Equals("image/tiff"));
                        break;
                    case "bmp":
                        encoder = encoders.Single(t => t.MimeType.Equals("image/bmp"));
                        break;
                    // TODO: this is dirty, defaulting to jpg but the return value of this thing is used all over the
                    // place so left it here, but it needs to not set a codec if it doesn't know which one to pick
                    // Note: when fixing this: both .jpg and .jpeg should be handled as extensions
                    default:
                        encoder = encoders.Single(t => t.MimeType.Equals("image/jpeg"));
                        break;
                }

                // set compresion ratio to 90%
                var encoderParams = new EncoderParameters();
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

                // save the new image
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, encoder, encoderParams);
                    stream.Seek(0, 0);
                    if (resizedFilepath.Contains("UMBRACOSYSTHUMBNAIL"))
                    {
                        var filepath = resizedFilepath.Replace("UMBRACOSYSTHUMBNAIL", maxWidthHeight.ToInvariantString());
                        AddFile(filepath, stream);
                        if (extension != "jpg")
                        {
                            filepath = filepath.TrimEnd(extension) + "jpg";
                            stream.Seek(0, 0);
                            AddFile(filepath, stream);
                        }
                        // TODO: Remove this, this is ONLY here for backwards compatibility but it is essentially completely unusable see U4-5385
                        stream.Seek(0, 0);
                        resizedFilepath = resizedFilepath.Replace("UMBRACOSYSTHUMBNAIL", width + "x" + height);
                    }

                    AddFile(resizedFilepath, stream);
                }

                return new ResizedImage(resizedFilepath, width, height);
            }
        }

        #endregion

        #region Inner classes

        [Obsolete("This should no longer be used, image manipulation should be done via ImageProcessor, Umbraco no longer generates '_thumb' files for media")]
        public class ResizedImage
        {
            public ResizedImage()
            { }

            public ResizedImage(string filepath, int width, int height)
            {
                Filepath = filepath;
                Width = width;
                Height = height;
            }

            public string Filepath { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        #endregion
    }
}
