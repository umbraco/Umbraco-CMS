using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Umbraco.Web.Security.Identity
{
    //NOTE: Not sure exactly what this is for but it is found in the AD source demo:
    // https://github.com/AzureADSamples/WebApp-WebAPI-OpenIDConnect-DotNet/blob/master/TodoListWebApp/Utils/NaiveSessionCache.cs
    // apparently it is needed for AD auth, so we'll put it here for people to use.
    // It would appear that this is better for whatever reason: https://github.com/OfficeDev/O365-WebApp-SingleTenant/blob/master/O365-WebApp-SingleTenant/Models/ADALTokenCache.cs
    // and please note that that link came from finding this thread: https://twitter.com/chakkaradeep/status/544962341528285184

    /// <summary>
    /// This is required to initialize the AD Identity provider on startup
    /// </summary>
    public class NaiveSessionCache : TokenCache
    {
        private static readonly object FileLock = new object();
        readonly string _userObjectId = string.Empty;
        readonly string _cacheId = string.Empty;
        public NaiveSessionCache(string userId)
        {
            _userObjectId = userId;
            _cacheId = _userObjectId + "_TokenCache";

            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            Load();
        }

        public void Load()
        {
            lock (FileLock)
            {
                this.Deserialize((byte[])HttpContext.Current.Session[_cacheId]);
            }
        }

        public void Persist()
        {
            lock (FileLock)
            {
                // reflect changes in the persistent store
                HttpContext.Current.Session[_cacheId] = this.Serialize();
                // once the write operation took place, restore the HasStateChanged bit to false
                this.HasStateChanged = false;
            }
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            System.Web.HttpContext.Current.Session.Remove(_cacheId);
        }

        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
            Persist();
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after ADAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (this.HasStateChanged)
            {
                Persist();
            }
        }
    }
}
