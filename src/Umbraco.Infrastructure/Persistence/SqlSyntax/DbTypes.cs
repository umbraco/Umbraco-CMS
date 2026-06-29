using System.Data;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

/// <summary>
/// Represents the set of database type definitions used for SQL syntax operations.
/// </summary>
public class DbTypes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.DbTypes"/> class with the specified type mappings.
    /// </summary>
    /// <param name="columnTypeMap">A read-only dictionary that maps CLR types to their corresponding database column type strings.</param>
    /// <param name="columnDbTypeMap">A read-only dictionary that maps CLR types to their corresponding <see cref="System.Data.DbType"/> values.</param>
    public DbTypes(IReadOnlyDictionary<Type, string> columnTypeMap, IReadOnlyDictionary<Type, DbType> columnDbTypeMap)
    {
        ColumnTypeMap = columnTypeMap;
        ColumnDbTypeMap = columnDbTypeMap;
    }

    /// <summary>
    /// Gets a read-only dictionary that maps CLR types to their corresponding SQL database column type strings, used for resolving database column types during SQL generation.
    /// </summary>
    public IReadOnlyDictionary<Type, string> ColumnTypeMap { get; }

    /// <summary>
    /// Gets a read-only dictionary that maps Common Language Runtime (CLR) types to their corresponding <see cref="DbType"/> values used in database operations.
    /// </summary>
    public IReadOnlyDictionary<Type, DbType> ColumnDbTypeMap { get; }
}
