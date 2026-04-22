using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
/// Represents the definition and properties of a database index, including its columns and constraints.
/// </summary>
public class IndexDefinition
{
    /// <summary>
    /// Gets or sets the name of the index.
    /// </summary>
    public virtual string? Name { get; set; }

    /// <summary>
    /// Gets or sets the schema name associated with the index.
    /// </summary>
    public virtual string? SchemaName { get; set; }

    /// <summary>
    /// Gets or sets the name of the database table to which this index definition applies.
    /// </summary>
    public virtual string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the name of the database column to which this index applies.
    /// </summary>
    public virtual string? ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the collection of <see cref="IndexColumnDefinition"/> objects that define the columns included in this index.
    /// </summary>
    public virtual ICollection<IndexColumnDefinition> Columns { get; set; } = new List<IndexColumnDefinition>();

    /// <summary>
    /// Gets or sets the collection of columns that are included in the index definition, but are not key columns. These columns are typically used to improve query performance by covering additional columns in the index.
    /// </summary>
    public virtual ICollection<IndexColumnDefinition> IncludeColumns { get; set; } = new List<IndexColumnDefinition>();

    /// <summary>
    /// Gets or sets the type of the database index, as defined by the <see cref="IndexTypes"/> enumeration.
    /// </summary>
    public IndexTypes IndexType { get; set; }
}
