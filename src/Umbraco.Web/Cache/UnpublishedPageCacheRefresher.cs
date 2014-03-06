using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher used for non-published content, this is primarily to notify Examine indexes to update
    /// </summary>
    public sealed class UnpublishedPageCacheRefresher : TypedCacheRefresherBase<UnpublishedPageCacheRefresher, IContent>
    {
        protected override UnpublishedPageCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.UnpublishedPageCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Unpublished Page Refresher"; }
        }

        //NOTE: There is no functionality for this cache refresher, it is here simply to emit events on each server for which examine
        // binds to. We could put the Examine index functionality in here but we've kept it all in the ExamineEvents class so that all of 
        // the logic is in one place. In the future we may put the examine logic in a cache refresher instead (that would make sense) but we'd
        // want to get this done before making more cache refreshers: 
        // http://issues.umbraco.org/issue/U4-2633
    }
}