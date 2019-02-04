using System;
using System.ComponentModel;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class UserGroupPermissionsCacheRefresher : CacheRefresherBase<UserGroupPermissionsCacheRefresher>
    {
        public UserGroupPermissionsCacheRefresher(AppCaches appCaches)
            : base(appCaches)
        { }

        #region Define

        protected override UserGroupPermissionsCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("840AB9C5-5C0B-48DB-A77E-29FE4B80CD3A");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "User Group Permissions Cache Refresher";

        #endregion

        #region Refresher

        #endregion
    }
}
