using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Core.Plugins
{
    internal static class TypeLoaderExtensions
    {
        /// <summary>
        /// Gets all classes inheriting from PropertyEditor.
        /// </summary>
        /// <remarks>
        /// <para>Excludes the actual PropertyEditor base type.</para>
        /// </remarks>
        public static IEnumerable<Type> GetPropertyEditors(this TypeLoader mgr)
        {
            // look for IParameterEditor (fast, IDiscoverable) then filter

            var propertyEditor = typeof (PropertyEditor);

            return mgr.GetTypes<IParameterEditor>()
                .Where(x => propertyEditor.IsAssignableFrom(x) && x != propertyEditor);
        }

        /// <summary>
        /// Gets all classes implementing IParameterEditor.
        /// </summary>
        /// <remarks>
        /// <para>Includes property editors.</para>
        /// <para>Excludes the actual ParameterEditor and PropertyEditor base types.</para>
        /// </remarks>
        public static IEnumerable<Type> GetParameterEditors(this TypeLoader mgr)
        {
            var propertyEditor = typeof (PropertyEditor);
            var parameterEditor = typeof (ParameterEditor);

            return mgr.GetTypes<IParameterEditor>()
                .Where(x => x != propertyEditor && x != parameterEditor);
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

        /// <summary>
        /// Gets all classes implementing ISqlSyntaxProvider and marked with the SqlSyntaxProviderAttribute.
        /// </summary>
        public static IEnumerable<Type> GetSqlSyntaxProviders(this TypeLoader mgr)
        {
            return mgr.GetTypesWithAttribute<ISqlSyntaxProvider, SqlSyntaxProviderAttribute>();
        }
    }
}