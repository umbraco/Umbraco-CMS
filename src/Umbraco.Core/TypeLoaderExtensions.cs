using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.PackageActions;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core
{
    public static class TypeLoaderExtensions
    {
        /// <summary>
        /// Gets all classes implementing <see cref="IDataEditor"/>.
        /// </summary>
        public static IEnumerable<Type> GetDataEditors(this TypeLoader mgr)
        {
            return mgr.GetTypes<IDataEditor>();
        }

        /// <summary>
        /// Gets all classes implementing ICacheRefresher.
        /// </summary>
        public static IEnumerable<Type> GetCacheRefreshers(this TypeLoader mgr)
        {
            return mgr.GetTypes<ICacheRefresher>();
        }

        /// <summary>
        /// Gets all classes implementing IPackageAction.
        /// </summary>
        public static IEnumerable<Type> GetPackageActions(this TypeLoader mgr)
        {
            return mgr.GetTypes<IPackageAction>();
        }
    }
}
