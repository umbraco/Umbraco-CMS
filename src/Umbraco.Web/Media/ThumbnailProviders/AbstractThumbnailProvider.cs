using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media.ThumbnailProviders
{
    public abstract class AbstractThumbnailProvider : IThumbnailProvider
    {
        protected abstract IEnumerable<string> SupportedExtensions { get; }

        public bool CanProvideThumbnail(string fileUrl)
        {
            string thumbUrl;
            return TryGetThumbnailUrl(fileUrl, out thumbUrl);
        }

        public string GetThumbnailUrl(string fileUrl)
        {
            string thumbUrl;
            TryGetThumbnailUrl(fileUrl, out thumbUrl);
            return thumbUrl;
        }

        protected bool IsSupportedExtension(string ext)
        {
            return SupportedExtensions.InvariantContains(ext) ||
                   SupportedExtensions.InvariantContains("*");
        }

        protected abstract bool TryGetThumbnailUrl(string fileUrl, out string thumbUrl);
    }
}
