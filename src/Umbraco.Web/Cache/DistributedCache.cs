using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Sync;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Represents the entry point into Umbraco's distributed cache infrastructure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The distributed cache infrastructure ensures that distributed caches are
    /// invalidated properly in load balancing environments.
    /// </para>
    /// <para>
    /// Distribute caches include static (in-memory) cache, runtime cache, front-end content cache, Examine/Lucene indexes
    /// </para>
    /// </remarks>
    public sealed class DistributedCache
    {
        #region Public constants/Ids

        public const string ApplicationTreeCacheRefresherId = "0AC6C028-9860-4EA4-958D-14D39F45886E";
        public const string ApplicationCacheRefresherId = "B15F34A1-BC1D-4F8B-8369-3222728AB4C8";
        public const string TemplateRefresherId = "DD12B6A0-14B9-46e8-8800-C154F74047C8";
        public const string PageCacheRefresherId = "27AB3022-3DFA-47b6-9119-5945BC88FD66";
        public const string UnpublishedPageCacheRefresherId = "55698352-DFC5-4DBE-96BD-A4A0F6F77145";
        public const string MemberCacheRefresherId = "E285DF34-ACDC-4226-AE32-C0CB5CF388DA";
        public const string MemberGroupCacheRefresherId = "187F236B-BD21-4C85-8A7C-29FBA3D6C00C";
        public const string MediaCacheRefresherId = "B29286DD-2D40-4DDB-B325-681226589FEC";
        public const string MacroCacheRefresherId = "7B1E683C-5F34-43dd-803D-9699EA1E98CA";
        public const string UserCacheRefresherId = "E057AF6D-2EE6-41F4-8045-3694010F0AA6";
        public const string UserPermissionsCacheRefresherId = "840AB9C5-5C0B-48DB-A77E-29FE4B80CD3A";
        public const string UserTypeCacheRefresherId = "7E707E21-0195-4522-9A3C-658CC1761BD4";
        public const string ContentTypeCacheRefresherId = "6902E22C-9C10-483C-91F3-66B7CAE9E2F5";
        public const string LanguageCacheRefresherId = "3E0F95D8-0BE5-44B8-8394-2B8750B62654";
        public const string DomainCacheRefresherId = "11290A79-4B57-4C99-AD72-7748A3CF38AF";
        public const string StylesheetCacheRefresherId = "E0633648-0DEB-44AE-9A48-75C3A55CB670";
        public const string StylesheetPropertyCacheRefresherId = "2BC7A3A4-6EB1-4FBC-BAA3-C9E7B6D36D38";
        public const string DataTypeCacheRefresherId = "35B16C25-A17E-45D7-BC8F-EDAB1DCC28D2";
        public const string DictionaryCacheRefresherId = "D1D7E227-F817-4816-BFE9-6C39B6152884";
        public const string PublicAccessCacheRefresherId = "1DB08769-B104-4F8B-850E-169CAC1DF2EC";

        public static readonly Guid ApplicationTreeCacheRefresherGuid = new Guid(ApplicationTreeCacheRefresherId);
        public static readonly Guid ApplicationCacheRefresherGuid = new Guid(ApplicationCacheRefresherId);
        public static readonly Guid TemplateRefresherGuid = new Guid(TemplateRefresherId);
        public static readonly Guid PageCacheRefresherGuid = new Guid(PageCacheRefresherId);
        public static readonly Guid UnpublishedPageCacheRefresherGuid = new Guid(UnpublishedPageCacheRefresherId);
        public static readonly Guid MemberCacheRefresherGuid = new Guid(MemberCacheRefresherId);
        public static readonly Guid MemberGroupCacheRefresherGuid = new Guid(MemberGroupCacheRefresherId);
        public static readonly Guid MediaCacheRefresherGuid = new Guid(MediaCacheRefresherId);
        public static readonly Guid MacroCacheRefresherGuid = new Guid(MacroCacheRefresherId);
        public static readonly Guid UserCacheRefresherGuid = new Guid(UserCacheRefresherId);
        public static readonly Guid UserPermissionsCacheRefresherGuid = new Guid(UserPermissionsCacheRefresherId);
        public static readonly Guid UserTypeCacheRefresherGuid = new Guid(UserTypeCacheRefresherId);
        public static readonly Guid ContentTypeCacheRefresherGuid = new Guid(ContentTypeCacheRefresherId);
        public static readonly Guid LanguageCacheRefresherGuid = new Guid(LanguageCacheRefresherId);
        public static readonly Guid DomainCacheRefresherGuid = new Guid(DomainCacheRefresherId);
        public static readonly Guid StylesheetCacheRefresherGuid = new Guid(StylesheetCacheRefresherId);
        public static readonly Guid StylesheetPropertyCacheRefresherGuid = new Guid(StylesheetPropertyCacheRefresherId);
        public static readonly Guid DataTypeCacheRefresherGuid = new Guid(DataTypeCacheRefresherId);
        public static readonly Guid DictionaryCacheRefresherGuid = new Guid(DictionaryCacheRefresherId);
        public static readonly Guid PublicAccessCacheRefresherGuid = new Guid(PublicAccessCacheRefresherId);

        #endregion

        #region Constructor & Singleton

        // note - should inject into the application instead of using a singleton
        private static readonly DistributedCache InstanceObject = new DistributedCache();

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCache"/> class.
        /// </summary>
        private DistributedCache()
        { }
        
        /// <summary>
        /// Gets the static unique instance of the <see cref="DistributedCache"/> class.
        /// </summary>
        /// <returns>The static unique instance of the <see cref="DistributedCache"/> class.</returns>
        /// <remarks>Exists so that extension methods can be added to the distributed cache.</remarks>
        public static DistributedCache Instance
        {
            get
            {
                return InstanceObject;    
            }
        }

        #endregion

        #region Core notification methods

        /// <summary>
        /// Notifies the distributed cache of specifieds item invalidation, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <typeparam name="T">The type of the invalidated items.</typeparam>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="getNumericId">A function returning the unique identifier of items.</param>
        /// <param name="instances">The invalidated items.</param>
        /// <remarks>
        /// This method is much better for performance because it does not need to re-lookup object instances.
        /// </remarks>
        public void Refresh<T>(Guid factoryGuid, Func<T, int> getNumericId, params T[] instances)
        {
            if (factoryGuid == Guid.Empty || instances.Length == 0 || getNumericId == null) return;

            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(factoryGuid),
                getNumericId,
                instances);
        }

        /// <summary>
        /// Notifies the distributed cache of a specified item invalidation, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="id">The unique identifier of the invalidated item.</param>
        public void Refresh(Guid factoryGuid, int id)
        {
            if (factoryGuid == Guid.Empty || id == default(int)) return;

            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations, 
                GetRefresherById(factoryGuid), 
                id);
        }

        /// <summary>
        /// Notifies the distributed cache of a specified item invalidation, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="id">The unique identifier of the invalidated item.</param>
        public void Refresh(Guid factoryGuid, Guid id)
        {
            if (factoryGuid == Guid.Empty || id == Guid.Empty) return;

            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(factoryGuid),
                id);
        }

        /// <summary>
        /// Notifies the distributed cache, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="jsonPayload">The notification content.</param>
        public void RefreshByJson(Guid factoryGuid, string jsonPayload)
        {
            if (factoryGuid == Guid.Empty || jsonPayload.IsNullOrWhiteSpace()) return;

            ServerMessengerResolver.Current.Messenger.PerformRefresh(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(factoryGuid),
                jsonPayload);
        }

        ///// <summary>
        ///// Notifies the distributed cache, for a specified <see cref="ICacheRefresher"/>.
        ///// </summary>
        ///// <param name="refresherId">The unique identifier of the ICacheRefresher.</param>
        ///// <param name="payload">The notification content.</param>
        //internal void Notify(Guid refresherId, object payload)
        //{
        //    if (refresherId == Guid.Empty || payload == null) return;

        //    ServerMessengerResolver.Current.Messenger.Notify(
        //        ServerRegistrarResolver.Current.Registrar.Registrations,
        //        GetRefresherById(refresherId),
        //        json);
        //}

        /// <summary>
        /// Notifies the distributed cache of a global invalidation for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher.</param>
        public void RefreshAll(Guid factoryGuid)
        {
            if (factoryGuid == Guid.Empty) return;
            RefreshAll(factoryGuid, true);
        }

        /// <summary>
        /// Notifies the distributed cache of a global invalidation for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="allServers">If true, all servers in the load balancing environment are notified; otherwise,
        /// only the local server is notified.</param>
        public void RefreshAll(Guid factoryGuid, bool allServers)
        {
            if (factoryGuid == Guid.Empty) return;

            ServerMessengerResolver.Current.Messenger.PerformRefreshAll(
                allServers 
                    ? ServerRegistrarResolver.Current.Registrar.Registrations
                    : Enumerable.Empty<IServerAddress>(), //this ensures it will only execute against the current server
                GetRefresherById(factoryGuid));
        }

        /// <summary>
        /// Notifies the distributed cache of a specified item removal, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="id">The unique identifier of the removed item.</param>
        public void Remove(Guid factoryGuid, int id)
        {
            if (factoryGuid == Guid.Empty || id == default(int)) return;

            ServerMessengerResolver.Current.Messenger.PerformRemove(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(factoryGuid),
                id);
        }

        /// <summary>
        /// Notifies the distributed cache of specifieds item removal, for a specified <see cref="ICacheRefresher"/>.
        /// </summary>
        /// <typeparam name="T">The type of the removed items.</typeparam>
        /// <param name="factoryGuid">The unique identifier of the ICacheRefresher.</param>
        /// <param name="getNumericId">A function returning the unique identifier of items.</param>
        /// <param name="instances">The removed items.</param>
        /// <remarks>
        /// This method is much better for performance because it does not need to re-lookup object instances.
        /// </remarks>
        public void Remove<T>(Guid factoryGuid, Func<T, int> getNumericId, params T[] instances)
        {
            ServerMessengerResolver.Current.Messenger.PerformRemove(
                ServerRegistrarResolver.Current.Registrar.Registrations,
                GetRefresherById(factoryGuid),
                getNumericId,
                instances);
        }

        #endregion

        // helper method to get an ICacheRefresher by its unique identifier
        private static ICacheRefresher GetRefresherById(Guid uniqueIdentifier)
        {
            return CacheRefreshersResolver.Current.GetById(uniqueIdentifier);
        }
    }
}
