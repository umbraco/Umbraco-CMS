using System.Collections.Concurrent;
using System.Reflection;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

public abstract class BaseMapper
{
    private readonly object _definedLock = new();

    private readonly MapperConfigurationStore _maps;

    // note: using a Lazy<ISqlContext> here because during installs, we are resolving the
    // mappers way before we have a configured IUmbracoDatabaseFactory, ie way before we
    // have an ISqlContext - this is some nasty temporal coupling which we might want to
    // cleanup eventually.
    private readonly Lazy<ISqlContext> _sqlContext;
    private bool _defined;

    private ISqlSyntaxProvider? _sqlSyntax;

    protected BaseMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
    {
        _sqlContext = sqlContext;
        _maps = maps;
    }

    internal string Map(string? propertyName)
    {
        lock (_definedLock)
        {
            if (!_defined)
            {
                ISqlContext? sqlContext = _sqlContext.Value;
                if (sqlContext == null)
                {
                    throw new InvalidOperationException("Could not get an ISqlContext.");
                }

                _sqlSyntax = sqlContext.SqlSyntax;

                DefineMaps();

                _defined = true;
            }
        }

        if (!_maps.TryGetValue(GetType(), out ConcurrentDictionary<string, string>? mapperMaps))
        {
            throw new InvalidOperationException($"No maps defined for mapper {GetType().FullName}.");
        }

        if (propertyName is null || !mapperMaps.TryGetValue(propertyName, out var mappedName))
        {
            throw new InvalidOperationException(
                $"No map defined by mapper {GetType().FullName} for property {propertyName}.");
        }

        return mappedName;
    }

    protected abstract void DefineMaps();

    // fixme: TSource is used for nothing
    protected void DefineMap<TSource, TTarget>(string sourceName, string targetName)
    {
        if (_sqlSyntax == null)
        {
            throw new InvalidOperationException("Do not define maps outside of DefineMaps.");
        }

        Type targetType = typeof(TTarget);

        // TODO ensure that sourceName is a valid sourceType property (but, slow?)
        TableNameAttribute? tableNameAttribute = targetType.FirstAttribute<TableNameAttribute>();
        if (tableNameAttribute == null)
        {
            throw new InvalidOperationException(
                $"Type {targetType.FullName} is not marked with a TableName attribute.");
        }

        var tableName = tableNameAttribute.Value;

        // TODO maybe get all properties once and then index them
        PropertyInfo? targetProperty = targetType.GetProperty(targetName);
        if (targetProperty == null)
        {
            throw new InvalidOperationException(
                $"Type {targetType.FullName} does not have a property named {targetName}.");
        }

        ColumnAttribute? columnAttribute = targetProperty.FirstAttribute<ColumnAttribute>();
        if (columnAttribute == null)
        {
            throw new InvalidOperationException(
                $"Property {targetType.FullName}.{targetName} is not marked with a Column attribute.");
        }

        var columnName = columnAttribute.Name;
        var columnMap = _sqlSyntax.GetQuotedTableName(tableName) + "." + _sqlSyntax.GetQuotedColumnName(columnName);

        ConcurrentDictionary<string, string> mapperMaps =
            _maps.GetOrAdd(GetType(), type => new ConcurrentDictionary<string, string>());
        mapperMaps[sourceName] = columnMap;
    }
}
