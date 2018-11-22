using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace umbraco.IO
{
    /// <summary>
    /// This class can be used to monitor file changes and update accordingly. This is copied
    /// from http://haacked.com/archive/2010/01/17/editable-routes.aspx and based on work in Dynamic Data
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in the future")]
    public class FileMonitor
    {
        private FileMonitor(Action<string> changeCallback)
            : this(HostingEnvironment.VirtualPathProvider, changeCallback)
        {
        }

        private FileMonitor(VirtualPathProvider vpp,
            Action<string> changeCallback)
        {
            _vpp = vpp;
            _changeCallback = changeCallback;
        }

        VirtualPathProvider _vpp;
        Action<string> _changeCallback;

        // When the file at the given path changes, 
        // we'll call the supplied action.
        public static void Listen(string virtualPath, Action<string> action)
        {
            var notifier = new FileMonitor(action);
            notifier.ListenForChanges(virtualPath);
        }

        void ListenForChanges(string virtualPath)
        {
            // Get a CacheDependency from the BuildProvider, 
            // so that we know anytime something changes
            var virtualPathDependencies = new List<string>();
            virtualPathDependencies.Add(virtualPath);
            CacheDependency cacheDependency = _vpp.GetCacheDependency(
              virtualPath, virtualPathDependencies, DateTime.UtcNow);
            HttpRuntime.Cache.Insert(virtualPath /*key*/,
              virtualPath /*value*/,
              cacheDependency,
              Cache.NoAbsoluteExpiration,
              Cache.NoSlidingExpiration,
              CacheItemPriority.NotRemovable,
              new CacheItemRemovedCallback(OnConfigFileChanged));
        }

        void OnConfigFileChanged(string key, object value,
          CacheItemRemovedReason reason)
        {
            // We only care about dependency changes
            if (reason != CacheItemRemovedReason.DependencyChanged)
                return;

            _changeCallback(key);

            // Need to listen for the next change
            ListenForChanges(key);
        }
    }
}
