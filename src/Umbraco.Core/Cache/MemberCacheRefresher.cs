//using Newtonsoft.Json;

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache
{
    public sealed class MemberCacheRefresher : PayloadCacheRefresherBase<MemberCacheRefresher, MemberCacheRefresher.JsonPayload>
    {
        private readonly IIdKeyMap _idKeyMap;

        public MemberCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IIdKeyMap idKeyMap)
            : base(appCaches, serializer)
        {
            _idKeyMap = idKeyMap;
        }

        public class JsonPayload
        {
            //[JsonConstructor]
            public JsonPayload(int id, string username, bool removed)
            {
                Id = id;
                Username = username;
                Removed = removed;
            }

            public int Id { get; }
            public string Username { get; }
            public bool Removed { get; }
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
            ClearCache(new JsonPayload(id, null, false));
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ClearCache(new JsonPayload(id, null, false));
            base.Remove(id);
        }

        private void ClearCache(params JsonPayload[] payloads)
        {
            AppCaches.ClearPartialViewCache();
            var memberCache = AppCaches.IsolatedCaches.Get<IMember>();

            foreach (var p in payloads)
            {
                _idKeyMap.ClearCache(p.Id);
                if (memberCache)
                {
                    memberCache.Result.Clear(RepositoryCacheKeys.GetKey<IMember>(p.Id));
                    memberCache.Result.Clear(RepositoryCacheKeys.GetKey<IMember>(p.Username));
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
    }
}
