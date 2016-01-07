﻿using System;
using System.Drawing;
using System.IO;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Media;

namespace Umbraco.Core.IO
{
    public class UmbracoMediaFile
    {
        private readonly MediaFileSystem _fs;

        #region Constructors

        public UmbracoMediaFile()
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
        }

        public UmbracoMediaFile(string path)
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

            Path = path;

            Initialize();
        }

        #endregion

        #region Static Methods

        //MB: Do we really need all these overloads? looking through the code, only one of them is actually used

        public static UmbracoMediaFile Save(HttpPostedFile file, string path)
        {
            return Save(file.InputStream, path);
        }

        public static UmbracoMediaFile Save(HttpPostedFileBase file, string path)
        {
            return Save(file.InputStream, path);
        }

        public static UmbracoMediaFile Save(Stream inputStream, string path)
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            fs.AddFile(path, inputStream);

            return new UmbracoMediaFile(path);
        }

        public static UmbracoMediaFile Save(byte[] file, string relativePath)
        {
            return Save(new MemoryStream(file), relativePath);
        }

        public static UmbracoMediaFile Save(HttpPostedFile file)
        {
            var tempDir = System.IO.Path.Combine("uploads", Guid.NewGuid().ToString());
            return Save(file, tempDir);
        }

        //filebase overload...
        public static UmbracoMediaFile Save(HttpPostedFileBase file)
        {
            var tempDir = System.IO.Path.Combine("uploads", Guid.NewGuid().ToString());
            return Save(file, tempDir);
        }

        #endregion

        private long? _length;
        private Size? _size;

        /// <summary>
        /// Initialized values that don't require opening the file.
        /// </summary>
        private void Initialize()
        {
            Filename = _fs.GetFileName(Path);
            Extension = _fs.GetExtension(Path) != null
                ? _fs.GetExtension(Path).TrimStart('.').ToLowerInvariant()
                : "";
            Url = _fs.GetUrl(Path);

            Exists = _fs.FileExists(Path);
            if (Exists == false)
                LogHelper.Warn<UmbracoMediaFile>("The media file doesn't exist: " + Path);
        }

        public bool Exists { get; private set; }

        public string Filename { get; private set; }

        public string Extension { get; private set; }

        public string Path { get; private set; }

        public string Url { get; private set; }

        /// <summary>
        /// Get the length of the file in bytes
        /// </summary>
        /// <remarks>
        /// We are lazy loading this, don't go opening the file on ctor like we were doing.
        /// </remarks>
        public long Length
        {
            get
            {
                if (_length != null) return _length.Value;
                _length = Exists ? _fs.GetSize(Path) : -1;
                return _length.Value;
            }
        }

        public bool SupportsResizing
        {
            get { return ImageHelper.IsImageFile(Extension); }
        }

        public string GetFriendlyName()
        {
            return Filename.SplitPascalCasing().ToFirstUpperInvariant();
        }

        public Size GetDimensions()
        {
            if (_size != null) return _size.Value;

            if (_fs.FileExists(Path))
            {
                EnsureFileSupportsResizing();

                using (var fs = _fs.OpenFile(Path))
                {
                    _size = ImageHelper.GetDimensions(fs);
                }
            }
            else
            {
                _size = new Size(-1, -1);
            }

            return _size.Value;
        }

        public string Resize(int width, int height)
        {
            if (Exists == false) return string.Empty;

            EnsureFileSupportsResizing();
            var filepath = Resize(width, height, -1, string.Empty);
            return _fs.GetUrl(filepath);
        }

        public string Resize(int maxWidthHeight, string filenameAddition)
        {
            if (Exists == false) return string.Empty;

            EnsureFileSupportsResizing();
            var filepath = Resize(-1, -1, maxWidthHeight, filenameAddition);
            return _fs.GetUrl(filepath);
        }

        private string Resize(int width, int height, int maxWidthHeight, string sizeName)
        {
            using (var filestream = _fs.OpenFile(Path))
            using (var image = Image.FromStream(filestream))
            {
                return ImageHelper.GenerateResized(_fs, image, Path, sizeName, maxWidthHeight, width, height).Filepath;
            }
        }

        private void EnsureFileSupportsResizing()
        {
            if (SupportsResizing == false)
                throw new InvalidOperationException(string.Format("The file {0} is not an image, so can't get dimensions", Filename));
        }
    }
}
