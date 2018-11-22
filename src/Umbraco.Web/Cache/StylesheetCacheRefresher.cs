using System;
using System.ComponentModel;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure stylesheet cache is refreshed when stylesheets change
    /// </summary>
    [Obsolete("This is no longer used and will be removed in future versions")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class StylesheetCacheRefresher : CacheRefresherBase<StylesheetCacheRefresher>
    {
        protected override StylesheetCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.StylesheetCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Stylesheet cache refresher"; }
        }

    }
}