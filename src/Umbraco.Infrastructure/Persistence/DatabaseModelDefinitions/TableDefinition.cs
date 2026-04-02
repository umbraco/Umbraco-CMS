namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
/// Defines the structure and metadata of a database table.
/// </summary>
public class TableDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableDefinition"/> class, which represents the definition of a database table in the Umbraco persistence layer.
    /// </summary>
    public TableDefinition()
    {
        Columns = new List<ColumnDefinition>();
        ForeignKeys = new List<ForeignKeyDefinition>();
        Indexes = new List<IndexDefinition>();
    }

    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the schema name of the table.
    /// </summary>
    public virtual string SchemaName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of column definitions for the table.
    /// </summary>
    public virtual ICollection<ColumnDefinition> Columns { get; set; }

    /// <summary>
    /// Gets or sets the collection of <see cref="ForeignKeyDefinition"/> objects that define the foreign keys for this table.
    /// </summary>
    public virtual ICollection<ForeignKeyDefinition> ForeignKeys { get; set; }

    /// <summary>
    /// Gets or sets the collection of index definitions associated with this table.
    /// </summary>
    public virtual ICollection<IndexDefinition> Indexes { get; set; }
}
