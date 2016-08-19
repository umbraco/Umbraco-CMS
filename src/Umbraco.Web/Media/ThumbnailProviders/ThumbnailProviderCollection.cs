using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media.ThumbnailProviders
{
    public class ThumbnailProviderCollection : BuilderCollectionBase<IThumbnailProvider>
    {
        public ThumbnailProviderCollection(IEnumerable<IThumbnailProvider> items)
            : base(items)
        { }

        public string GetThumbnailUrl(string fileUrl)
        {
                var provider = this.FirstOrDefault(x => x.CanProvideThumbnail(fileUrl));
                return provider != null ? provider.GetThumbnailUrl(fileUrl) : string.Empty;
        }
    }
}
