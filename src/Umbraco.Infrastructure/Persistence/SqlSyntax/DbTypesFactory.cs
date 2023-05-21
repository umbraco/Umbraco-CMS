using System.Data;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

internal class DbTypesFactory
{
    private readonly Dictionary<Type, DbType> _columnDbTypeMap = new();
    private readonly Dictionary<Type, string> _columnTypeMap = new();

    public void Set<T>(DbType dbType, string fieldDefinition)
    {
        _columnTypeMap[typeof(T)] = fieldDefinition;
        _columnDbTypeMap[typeof(T)] = dbType;
    }

    public DbTypes Create() => new(_columnTypeMap, _columnDbTypeMap);
}
