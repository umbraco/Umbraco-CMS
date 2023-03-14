using System.Data;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

public class DbTypes
{
    public DbTypes(IReadOnlyDictionary<Type, string> columnTypeMap, IReadOnlyDictionary<Type, DbType> columnDbTypeMap)
    {
        ColumnTypeMap = columnTypeMap;
        ColumnDbTypeMap = columnDbTypeMap;
    }

    public IReadOnlyDictionary<Type, string> ColumnTypeMap { get; }

    public IReadOnlyDictionary<Type, DbType> ColumnDbTypeMap { get; }
}
