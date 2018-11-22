using System;
using System.ComponentModel;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Cache
{
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class UserGroupPermissionsCacheRefresher : CacheRefresherBase<UserGroupPermissionsCacheRefresher>
    {
        protected override UserGroupPermissionsCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return Guid.Parse(DistributedCache.UserGroupPermissionsCacheRefresherId); }
        }


        public override string Name
        {
            get { return "User group permissions cache refresher"; }
        }
        
    }
}