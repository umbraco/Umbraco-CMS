using System;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Services;

namespace Umbraco.Web.Cache
{
    public sealed class MemberGroupCacheRefresher : JsonCacheRefresherBase<MemberGroupCacheRefresher>
    {
        private readonly IMemberGroupService _memberGroupService;

        public MemberGroupCacheRefresher(CacheHelper cacheHelper, IMemberGroupService memberGroupService)
            : base(cacheHelper)
        {
            _memberGroupService = memberGroupService;
        }

        #region Define

        protected override MemberGroupCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("187F236B-BD21-4C85-8A7C-29FBA3D6C00C");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Member Group Cache Refresher";

        #endregion

        #region Refresher

        public override void Refresh(string json)
        {
            var payload = Deserialize(json);
            ClearCache(payload);
            base.Refresh(json);
        }

        public override void Refresh(int id)
        {
            var group = _memberGroupService.GetById(id);
            if (group != null)
                ClearCache(new JsonPayload(group.Id, group.Name));
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            var group = _memberGroupService.GetById(id);
            if (group != null)
                ClearCache(new JsonPayload(group.Id, group.Name));
            base.Remove(id);
        }

        private void ClearCache(params JsonPayload[] payloads)
        {
            if (payloads == null) return;

            var memberGroupCache = CacheHelper.IsolatedRuntimeCache.GetCache<IMemberGroup>();
            if (memberGroupCache == false) return;

            foreach (var payload in payloads.WhereNotNull())
            {
                memberGroupCache.Result.ClearCacheByKeySearch($"{typeof(IMemberGroup).FullName}.{payload.Name}");
                memberGroupCache.Result.ClearCacheItem(RepositoryCacheKeys.GetKey<IMemberGroup>(payload.Id));
            }
        }

        #endregion

        #region Json

        public class JsonPayload
        {
            public JsonPayload(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public string Name { get; }
            public int Id { get; }
        }

        private JsonPayload[] Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<JsonPayload[]>(json);
        }

        internal static string Serialize(params IMemberGroup[] groups)
        {
            return JsonConvert.SerializeObject(groups.Select(x => new JsonPayload(x.Id, x.Name)).ToArray());
        }

        #endregion
    }
}
