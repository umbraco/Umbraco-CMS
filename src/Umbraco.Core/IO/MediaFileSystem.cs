using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Media;

namespace Umbraco.Core.IO
{
	/// <summary>
	/// A custom file system provider for media
	/// </summary>
	[FileSystemProvider("media")]
	public class MediaFileSystem : FileSystemWrapper2
	{
	    private readonly IContentSection _contentConfig;

	    public MediaFileSystem(IFileSystem2 wrapped)
			: this(wrapped, UmbracoConfig.For.UmbracoSettings().Content)
		{
		}

        public MediaFileSystem(IFileSystem2 wrapped, IContentSection contentConfig) : base(wrapped)
        {
            _contentConfig = contentConfig;
        }

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

        [Obsolete("Use ImageHelper.GetThumbnails instead.", false)]
		public IEnumerable<string> GetThumbnails(string path)
        {
            return ImageHelper.GetThumbnails(this, path);
		}

        [Obsolete("Use ImageHelper.DeleteFile instead.", false)]
        public void DeleteFile(string path, bool deleteThumbnails)
		{
            ImageHelper.DeleteFile(this, path, deleteThumbnails);
		}

        [Obsolete("Use ImageHelper.DeleteThumbnails instead.", false)]
        public void DeleteThumbnails(string path)
		{
            ImageHelper.DeleteThumbnails(this, path);
		}

        [Obsolete("Use ImageHelper.CopyThumbnails instead.", false)]
        public void CopyThumbnails(string sourcePath, string targetPath)
	    {
            ImageHelper.CopyThumbnails(this, sourcePath, targetPath);
        }
	}
}
