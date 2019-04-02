using System;
using System.Collections.Concurrent;
using NPoco;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Persistence.Mappers
{
    public abstract class BaseMapper : IDiscoverable
    {
        // note: using a Lazy<ISqlContext> here because during installs, we are resolving the
        // mappers way before we have a configured IUmbracoDatabaseFactory, ie way before we
        // have an ISqlContext - this is some nasty temporal coupling which we might want to
        // cleanup eventually.

        private readonly Lazy<ISqlContext> _sqlContext;
        private readonly object _definedLock = new object();
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> _maps;

        private ISqlSyntaxProvider _sqlSyntax;
        private bool _defined;

        protected BaseMapper(Lazy<ISqlContext> sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
        {
            _sqlContext = sqlContext;
            _maps = maps;
        }

        protected abstract void DefineMaps();

        internal string Map(string propertyName, bool throws = false)
        {
            lock (_definedLock)
            {
                if (!_defined)
                {
                    var sqlContext = _sqlContext.Value;
                    if (sqlContext == null)
                        throw new InvalidOperationException("Could not get an ISqlContext.");
                    _sqlSyntax = sqlContext.SqlSyntax;

                    DefineMaps();

                    _defined = true;
                }
            }

            if (!_maps.TryGetValue(GetType(), out var mapperMaps))
                throw new InvalidOperationException($"No maps defined for mapper {GetType().FullName}.");
            if (!mapperMaps.TryGetValue(propertyName, out var mappedName))
                throw new InvalidOperationException($"No map defined by mapper {GetType().FullName} for property {propertyName}.");
            return mappedName;
        }

        protected void DefineMap<TSource, TTarget>(string sourceName, string targetName)
        {
            if (_sqlSyntax == null)
                throw new InvalidOperationException("Do not define maps outside of DefineMaps.");

            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            // TODO ensure that sourceName is a valid sourceType property (but, slow?)

            var tableNameAttribute = targetType.FirstAttribute<TableNameAttribute>();
            if (tableNameAttribute == null) throw new InvalidOperationException($"Type {targetType.FullName} is not marked with a TableName attribute.");
            var tableName = tableNameAttribute.Value;

            // TODO maybe get all properties once and then index them
            var targetProperty = targetType.GetProperty(targetName);
            if (targetProperty == null) throw new InvalidOperationException($"Type {targetType.FullName} does not have a property named {targetName}.");
            var columnAttribute = targetProperty.FirstAttribute<ColumnAttribute>();
            if (columnAttribute == null) throw new InvalidOperationException($"Property {targetType.FullName}.{targetName} is not marked with a Column attribute.");

            var columnName = columnAttribute.Name;
            var columnMap = _sqlSyntax.GetQuotedTableName(tableName) + "." + _sqlSyntax.GetQuotedColumnName(columnName);

            var mapperMaps = _maps.GetOrAdd(GetType(), type => new ConcurrentDictionary<string, string>());
            mapperMaps[sourceName] = columnMap;
        }
    }
}
