using System;
using System.ComponentModel;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure stylesheet property cache is refreshed when stylesheet properties change
    /// </summary>
    [Obsolete("This is no longer used and will be removed in future versions")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class StylesheetPropertyCacheRefresher : CacheRefresherBase<StylesheetPropertyCacheRefresher>
    {
        protected override StylesheetPropertyCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.StylesheetPropertyCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Stylesheet property cache refresher"; }
        }
        
    }
}