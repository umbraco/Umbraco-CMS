using System;
using System.Web.Script.Serialization;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using umbraco;

using Umbraco.Core.Persistence.Repositories;
using umbraco.interfaces;
using System.Linq;
using Macro = umbraco.cms.businesslogic.macro.Macro;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure macro cache is updated when members change
    /// </summary>
    /// <remarks>
    /// This is not intended to be used directly in your code and it should be sealed but due to legacy code we cannot seal it.
    /// </remarks>
    public class MacroCacheRefresher : JsonCacheRefresherBase<MacroCacheRefresher>
    {
        #region Static helpers
        
        internal static string[] GetAllMacroCacheKeys()
        {
            return new[]
                {
                    CacheKeys.MacroCacheKey,
                    CacheKeys.MacroControlCacheKey,
                    CacheKeys.MacroHtmlCacheKey,
                    CacheKeys.MacroHtmlDateAddedCacheKey,
                    CacheKeys.MacroControlDateAddedCacheKey,
                    CacheKeys.MacroXsltCacheKey,
                };
        }

        internal static string[] GetCacheKeysForAlias(string alias)
        {
            return GetAllMacroCacheKeys().Select(x => x + alias).ToArray();
        }

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
        /// <param name="macros"></param>
        /// <returns></returns>
        internal static string SerializeToJsonPayload(params Macro[] macros)
        {
            var serializer = new JavaScriptSerializer();
            var items = macros.Select(FromMacro).ToArray();
            var json = serializer.Serialize(items);
            return json;
        }

        /// <summary>
        /// Creates the custom Json payload used to refresh cache amongst the servers
        /// </summary>
        /// <param name="macros"></param>
        /// <returns></returns>
        internal static string SerializeToJsonPayload(params IMacro[] macros)
        {
            var serializer = new JavaScriptSerializer();
            var items = macros.Select(FromMacro).ToArray();
            var json = serializer.Serialize(items);
            return json;
        }

        /// <summary>
        /// Creates the custom Json payload used to refresh cache amongst the servers
        /// </summary>
        /// <param name="macros"></param>
        /// <returns></returns>
        internal static string SerializeToJsonPayload(params macro[] macros)
        {
            var serializer = new JavaScriptSerializer();
            var items = macros.Select(FromMacro).ToArray();
            var json = serializer.Serialize(items);
            return json;
        }

        /// <summary>
        /// Converts a macro to a jsonPayload object
        /// </summary>
        /// <param name="macro"></param>
        /// <returns></returns>
        private static JsonPayload FromMacro(IMacro macro)
        {
            var payload = new JsonPayload
            {
                Alias = macro.Alias,
                Id = macro.Id
            };
            return payload;
        }

        /// <summary>
        /// Converts a macro to a jsonPayload object
        /// </summary>
        /// <param name="macro"></param>
        /// <returns></returns>
        private static JsonPayload FromMacro(Macro macro)
        {
            var payload = new JsonPayload
            {
                Alias = macro.Alias,
                Id = macro.Id
            };            
            return payload;
        }

        /// <summary>
        /// Converts a macro to a jsonPayload object
        /// </summary>
        /// <param name="macro"></param>
        /// <returns></returns>
        private static JsonPayload FromMacro(macro macro)
        {
            var payload = new JsonPayload
            {
                Alias = macro.Alias,
                Id = macro.Model.Id
            };
            return payload;
        }

        #endregion

        #region Sub classes

        private class JsonPayload
        {            
            public string Alias { get; set; }
            public int Id { get; set; }
        }

        #endregion

        protected override MacroCacheRefresher Instance
        {
            get { return this; }
        }

        public override string Name
        {
            get
            {
                return "Macro cache refresher";
            }
        }

        public override Guid UniqueIdentifier
        {
            get
            {
                return new Guid(DistributedCache.MacroCacheRefresherId);
            }
        }

        public override void RefreshAll()
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<MacroCacheContent>();
            GetAllMacroCacheKeys().ForEach(
                    prefix =>
                    ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(prefix));

            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IMacro>();

            base.RefreshAll();
        }

        public override void Refresh(string jsonPayload)
        {
            var payloads = DeserializeFromJsonPayload(jsonPayload);

            payloads.ForEach(payload =>
            {
                GetCacheKeysForAlias(payload.Alias).ForEach(
                    alias =>
                    ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(alias));

                ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(RepositoryBase.GetCacheIdKey<IMacro>(payload.Id));
            });

            base.Refresh(jsonPayload);
        }

    }
}