using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Services;

namespace Umbraco.Web.Cache
{
    public sealed class MemberCacheRefresher : PayloadCacheRefresherBase<MemberCacheRefresher, MemberCacheRefresher.JsonPayload>
    {
        private readonly IdkMap _idkMap;
        private readonly LegacyMemberCacheRefresher _legacyMemberRefresher;

        public MemberCacheRefresher(AppCaches appCaches, IdkMap idkMap)
            : base(appCaches)
        {
            _idkMap = idkMap;
            _legacyMemberRefresher = new LegacyMemberCacheRefresher(this, appCaches);
        }

        public class JsonPayload
        {
            [JsonConstructor]
            public JsonPayload(int id, string username)
            {
                Id = id;
                Username = username;
            }

            public int Id { get; }
            public string Username { get; }

            // TODO: In netcore change this to be get only and adjust the ctor. We cannot do that now since that
            // is a breaking change due to only having a single jsonconstructor allowed.
            public bool Removed { get; set; }

        }

        #region Define

        protected override MemberCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("E285DF34-ACDC-4226-AE32-C0CB5CF388DA");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Member Cache Refresher";

        #endregion

        #region Refresher

        public override void Refresh(JsonPayload[] payloads)
        {
            ClearCache(payloads);
            base.Refresh(payloads);
        }

        public override void Refresh(int id)
        {
            ClearCache(new JsonPayload(id, null));
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ClearCache(new JsonPayload(id, null));
            base.Remove(id);
        }

        [Obsolete("This is no longer used and will be removed from the codebase in the future")]
        public void Refresh(IMember instance) => _legacyMemberRefresher.Refresh(instance);

        [Obsolete("This is no longer used and will be removed from the codebase in the future")]
        public void Remove(IMember instance) => _legacyMemberRefresher.Remove(instance);

        private void ClearCache(params JsonPayload[] payloads)
        {
            AppCaches.ClearPartialViewCache();
            var memberCache = AppCaches.IsolatedCaches.Get<IMember>();

            foreach (var p in payloads)
            {
                _idkMap.ClearCache(p.Id);
                if (memberCache)
                {
                    memberCache.Result.Clear(RepositoryCacheKeys.GetKey<IMember, int>(p.Id));
                    memberCache.Result.Clear(RepositoryCacheKeys.GetKey<IMember, string>(p.Username));
                }
            }

        }

        #endregion

        #region Indirect

        public static void RefreshMemberTypes(AppCaches appCaches)
        {
            appCaches.IsolatedCaches.ClearCache<IMember>();
        }

        #endregion

        #region Backwards Compat

        // TODO: this is here purely for backwards compat but should be removed in netcore
        private class LegacyMemberCacheRefresher : TypedCacheRefresherBase<MemberCacheRefresher, IMember>
        {
            private readonly MemberCacheRefresher _parent;

            public LegacyMemberCacheRefresher(MemberCacheRefresher parent, AppCaches appCaches) : base(appCaches)
            {
                _parent = parent;
            }

            public override Guid RefresherUniqueId => _parent.RefresherUniqueId;

            public override string Name => _parent.Name;

            protected override MemberCacheRefresher This => _parent;

            public override void Refresh(IMember instance)
            {
                _parent.ClearCache(new JsonPayload(instance.Id, instance.Username));
                base.Refresh(instance.Id);
            }

            public override void Remove(IMember instance)
            {
                _parent.ClearCache(new JsonPayload(instance.Id, instance.Username));
                base.Remove(instance);
            }
        }

        #endregion
    }
}
