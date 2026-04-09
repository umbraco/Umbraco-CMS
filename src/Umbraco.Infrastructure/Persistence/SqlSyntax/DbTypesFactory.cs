using System.Data;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

internal sealed class DbTypesFactory
{
    private readonly Dictionary<Type, DbType> _columnDbTypeMap = new();
    private readonly Dictionary<Type, string> _columnTypeMap = new();

    /// <summary>
    /// Sets the database type and field definition for the specified generic type.
    /// </summary>
    /// <param name="dbType">The database type to associate with the generic type.</param>
    /// <param name="fieldDefinition">The field definition string for the database column.</param>
    public void Set<T>(DbType dbType, string fieldDefinition)
    {
        _columnTypeMap[typeof(T)] = fieldDefinition;
        _columnDbTypeMap[typeof(T)] = dbType;
    }

    /// <summary>
    /// Creates and returns a new <see cref="Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.DbTypes"/> instance.
    /// </summary>
    /// <returns>A new <see cref="Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.DbTypes"/> object.</returns>
    public DbTypes Create() => new(_columnTypeMap, _columnDbTypeMap);
}
