namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
///     Represents a database index definition retrieved by querying the database
/// </summary>
internal class DbIndexDefinition
{
    public DbIndexDefinition(Tuple<string, string, string, bool> data)
    {
        TableName = data.Item1;
        IndexName = data.Item2;
        ColumnName = data.Item3;
        IsUnique = data.Item4;
    }

    public string IndexName { get; }

    public string TableName { get; }

    public string ColumnName { get; }

    public bool IsUnique { get; }
}
