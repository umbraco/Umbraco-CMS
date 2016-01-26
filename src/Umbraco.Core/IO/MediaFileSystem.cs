using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.IO
{
	/// <summary>
	/// A custom file system provider for media
	/// </summary>
	[FileSystemProvider("media")]
	public class MediaFileSystem : FileSystemWrapper
	{
	    private readonly IContentSection _contentConfig;

	    public MediaFileSystem(IFileSystem wrapped)
			: this(wrapped, UmbracoConfig.For.UmbracoSettings().Content)
		{
		}

        public MediaFileSystem(IFileSystem wrapped, IContentSection contentConfig) : base(wrapped)
        {
            _contentConfig = contentConfig;
        }

        [Obsolete("This low-level method should NOT exist.")]
        public string GetRelativePath(int propertyId, string fileName)
		{
            var sep = _contentConfig.UploadAllowDirectories
				? Path.DirectorySeparatorChar
				: '-';

			return propertyId.ToString(CultureInfo.InvariantCulture) + sep + fileName;
		}

        [Obsolete("This low-level method should NOT exist.")]
        public string GetRelativePath(string subfolder, string fileName)
        {
            var sep = _contentConfig.UploadAllowDirectories
                ? Path.DirectorySeparatorChar
                : '-';

            return subfolder + sep + fileName;
        }

        // what's below is weird
        // we are not deleting custom thumbnails
        // MediaFileSystem is not just IFileSystem
        // etc

        [Obsolete("", true)]
		public IEnumerable<string> GetThumbnails(string path)
		{
			var parentDirectory = Path.GetDirectoryName(path);
			var extension = Path.GetExtension(path);

			return GetFiles(parentDirectory)
				.Where(x => x.StartsWith(path.TrimEnd(extension) + "_thumb") || x.StartsWith(path.TrimEnd(extension) + "_big-thumb"))
				.ToList();
		}

        [Obsolete("", true)]
        public void DeleteFile(string path, bool deleteThumbnails)
		{
			DeleteFile(path);

			if (deleteThumbnails == false)
				return;

			DeleteThumbnails(path);
		}

        [Obsolete("", true)]
        public void DeleteThumbnails(string path)
		{
			GetThumbnails(path)
				.ForEach(DeleteFile);
		}

        [Obsolete("", true)]
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
	}
}
