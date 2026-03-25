namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
///     Represents a database index definition retrieved by querying the database
/// </summary>
internal sealed class DbIndexDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DbIndexDefinition"/> class using the specified tuple.
    /// </summary>
    /// <param name="data">
    /// A tuple containing the index definition data:
    /// Item1 is the column name, Item2 is the index name, Item3 is the index type, and Item4 is a boolean indicating whether the index is unique.
    /// </param>
    public DbIndexDefinition(Tuple<string, string, string, bool> data)
    {
        TableName = data.Item1;
        IndexName = data.Item2;
        ColumnName = data.Item3;
        IsUnique = data.Item4;
    }

    /// <summary>
    /// Gets the name of the index.
    /// </summary>
    public string IndexName { get; }

    /// <summary>
    /// Gets the name of the table to which this index belongs.
    /// </summary>
    public string TableName { get; }

    /// <summary>
    /// Gets the name of the database column that is part of this index definition.
    /// </summary>
    public string ColumnName { get; }

    /// <summary>
    /// Gets a value indicating whether the index is unique.
    /// </summary>
    public bool IsUnique { get; }
}
