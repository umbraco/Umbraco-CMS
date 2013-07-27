using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.Files
{
    [Obsolete("Use Umbraco.Core.IO.UmbracoMediaFile instead")]
    public class UmbracoFile : IFile
    {
        private readonly UmbracoMediaFile _mediaFile;
        
        #region Constructors

        public UmbracoFile()
        {
            _mediaFile = new UmbracoMediaFile();
        }

        public UmbracoFile(string path)
        {
            _mediaFile = new UmbracoMediaFile(path);
        }

        internal UmbracoFile(UmbracoMediaFile mediaFile)
        {
            _mediaFile = mediaFile;
        }

        #endregion

        #region Static Methods

        //MB: Do we really need all these overloads? looking through the code, only one of them is actually used

        public static UmbracoFile Save(HttpPostedFile file, string path)
        {
            return new UmbracoFile(UmbracoMediaFile.Save(file.InputStream, path));
        }

        public static UmbracoFile Save(HttpPostedFileBase file, string path)
        {
            return new UmbracoFile(UmbracoMediaFile.Save(file.InputStream, path));
        }

        public static UmbracoFile Save(Stream inputStream, string path)
        {
            return new UmbracoFile(UmbracoMediaFile.Save(inputStream, path));
        }

        public static UmbracoFile Save(byte[] file, string relativePath)
        {
            return new UmbracoFile(UmbracoMediaFile.Save(new MemoryStream(file), relativePath));
        }

        public static UmbracoFile Save(HttpPostedFile file)
        {
            return new UmbracoFile(UmbracoMediaFile.Save(file));
        }

        //filebase overload...
        public static UmbracoFile Save(HttpPostedFileBase file)
        {
            return new UmbracoFile(UmbracoMediaFile.Save(file));
        }

        #endregion
        
        public string Filename
        {
            get { return _mediaFile.Filename; }
        }

        public string Extension
        {
            get { return _mediaFile.Extension; }
        }

        [Obsolete("LocalName is obsolete, please use Url instead", false)]
        public string LocalName
        {
            get { return Url; }
        }

        public string Path
        {
            get { return _mediaFile.Path; }
        }

        public string Url
        {
            get { return _mediaFile.Url; }
        }

        public long Length
        {
            get { return _mediaFile.Length; }
        }

        public bool SupportsResizing
        {
            get { return _mediaFile.SupportsResizing; }
        }

        public string GetFriendlyName()
        {
            return _mediaFile.GetFriendlyName();
        }

        public System.Tuple<int, int> GetDimensions()
        {
            var size = _mediaFile.GetDimensions();
            return new System.Tuple<int, int>(size.Width, size.Height);
        }

        public string Resize(int width, int height)
        {
            return _mediaFile.Resize(width, height);
        }

        public string Resize(int maxWidthHeight, string fileNameAddition)
        {
            return _mediaFile.Resize(maxWidthHeight, fileNameAddition);
        }

    }

}
