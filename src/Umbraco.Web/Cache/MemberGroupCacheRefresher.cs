using System;
using System.Linq;
using System.Web.Script.Serialization;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;

using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Web.Cache
{
    public sealed class MemberGroupCacheRefresher : JsonCacheRefresherBase<MemberGroupCacheRefresher>
    {
        #region Static helpers

        /// <summary>
        /// Converts the json to a JsonPayload object
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static JsonPayload[] DeserializeFromJsonPayload(string json)
        {
            var serializer = new JavaScriptSerializer();
            var jsonObject = serializer.Deserialize<JsonPayload[]>(json);
            return jsonObject;
        }

        /// <summary>
        /// Creates the custom Json payload used to refresh cache amongst the servers
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        internal static string SerializeToJsonPayload(params IMemberGroup[] groups)
        {
            var serializer = new JavaScriptSerializer();
            var items = groups.Select(FromMemberGroup).ToArray();
            var json = serializer.Serialize(items);
            return json;
        }

        /// <summary>
        /// Converts a macro to a jsonPayload object
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private static JsonPayload FromMemberGroup(IMemberGroup group)
        {
            if (group == null) return null;

            var payload = new JsonPayload
            {
                Id = group.Id,
                Name = group.Name
            };
            return payload;
        }

        #endregion

        #region Sub classes

        private class JsonPayload
        {
            public string Name { get; set; }
            public int Id { get; set; }
        }

        #endregion

        protected override MemberGroupCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.MemberGroupCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Clears Member Group Cache"; }
        }

        public override void Refresh(string jsonPayload)
        {
            ClearCache(DeserializeFromJsonPayload(jsonPayload));
            base.Refresh(jsonPayload);
        }

        public override void Refresh(int id)
        {
            ClearCache(FromMemberGroup(ApplicationContext.Current.Services.MemberGroupService.GetById(id)));
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ClearCache(FromMemberGroup(ApplicationContext.Current.Services.MemberGroupService.GetById(id)));
            base.Remove(id);
        }

        private void ClearCache(params JsonPayload[] payloads)
        {
            if (payloads == null) return;

            payloads.ForEach(payload =>
            {
                if (payload != null)
                {
                    ApplicationContext.Current.ApplicationCache.RuntimeCache
                        .ClearCacheByKeySearch(string.Format("{0}.{1}", typeof(IMemberGroup).FullName, payload.Name));
                    ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(RepositoryBase.GetCacheIdKey<IMemberGroup>(payload.Id));
                }                
            });
            
        }
    }
}