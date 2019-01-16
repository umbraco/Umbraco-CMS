using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Core.Composing
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

        /// <summary>
        /// Gets all classes inheriting from BaseMapper and marked with the MapperForAttribute.
        /// </summary>
        public static IEnumerable<Type> GetAssignedMapperTypes(this TypeLoader mgr)
        {
            return mgr.GetTypesWithAttribute<BaseMapper, MapperForAttribute>();
        }
    }
}
