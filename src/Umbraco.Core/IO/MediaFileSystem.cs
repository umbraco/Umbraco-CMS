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

	    public string GetRelativePath(int propertyId, string fileName)
		{
            var seperator = _contentConfig.UploadAllowDirectories
				? Path.DirectorySeparatorChar
				: '-';

			return propertyId.ToString(CultureInfo.InvariantCulture) + seperator + fileName;
		}

        public string GetRelativePath(string subfolder, string fileName)
        {
            var seperator = _contentConfig.UploadAllowDirectories
                ? Path.DirectorySeparatorChar
                : '-';

            return subfolder + seperator + fileName;
        }

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
			DeleteFile(path);

			if (deleteThumbnails == false)
				return;

			DeleteThumbnails(path);
		}

		public void DeleteThumbnails(string path)
		{
			GetThumbnails(path)
				.ForEach(DeleteFile);
		}
	}
}
