using System;
using System.Collections.Generic;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.PackageActions;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core
{
    internal static class TypeLoaderExtensions
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
