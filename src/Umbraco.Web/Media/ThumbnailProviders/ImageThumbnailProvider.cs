using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.IO;

namespace Umbraco.Web.Media.ThumbnailProviders
{
	[WeightedPlugin(1000)]
    public class ImageThumbnailProvider : AbstractThumbnailProvider
    {        
        protected override IEnumerable<string> SupportedExtensions
        {
            get { return new List<string> { ".jpeg", ".jpg", ".gif", ".bmp", ".png", ".tiff", ".tif" }; }
        }

        protected override bool TryGetThumbnailUrl(string fileUrl, out string thumbUrl)
        {
            // Set thumbnail url to empty strin initially
            thumbUrl = string.Empty;

            // Make sure file has an extension
            var ext = Path.GetExtension(fileUrl);
            if (string.IsNullOrEmpty(ext))
                return false;

            // Make sure it has a supported file extension
            if (!IsSupportedExtension(ext))
                return false;

            // Make sure the thumbnail exists
            var tmpThumbUrl = fileUrl.Replace(ext, "_thumb.jpg");

            try
            {
                var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
                var relativeThumbPath = fs.GetRelativePath(tmpThumbUrl);
                if (!fs.FileExists(relativeThumbPath))
                    return false;
            }
            catch (Exception)
            {
                // If something odd happens, just return false and move on
                return false;
            }

            // We've got this far, so thumbnail must exist
            thumbUrl = tmpThumbUrl;
            return true;
        }
    }
}
