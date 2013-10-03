using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.IO;

namespace Umbraco.Web.Media.ThumbnailProviders
{
	[WeightedPlugin(2000)]
    public class FileExtensionIconThumbnailProvider : AbstractThumbnailProvider
    {
        protected override IEnumerable<string> SupportedExtensions
        {
            get { return new List<string> { "*" }; }
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
            var tmpThumbUrl = IOHelper.ResolveUrl(SystemDirectories.Umbraco + "/images/mediaThumbnails/"+ ext.TrimStart('.') +".png");
            if (!File.Exists(IOHelper.MapPath(tmpThumbUrl)))
                return false;

            // We've got this far, so thumbnail must exist
            thumbUrl = tmpThumbUrl;
            return true;
        }
    }
}
