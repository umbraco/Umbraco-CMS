using System.Data;

namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
/// Represents the definition of a foreign key constraint in the database model.
/// </summary>
public class ForeignKeyDefinition
{
    /// <summary>
    /// Initializes a new, empty instance of the <see cref="ForeignKeyDefinition"/> class.
    /// </summary>
    public ForeignKeyDefinition()
    {
        ForeignColumns = new List<string>();
        PrimaryColumns = new List<string>();

        // Set to None by Default
        OnDelete = Rule.None;
        OnUpdate = Rule.None;
    }

    /// <summary>
    /// Gets or sets the name of the foreign key.
    /// </summary>
    public virtual string? Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the referenced (foreign) table in the foreign key relationship.
    /// </summary>
    public virtual string? ForeignTable { get; set; }

    /// <summary>
    /// Gets or sets the schema name of the foreign table.
    /// </summary>
    public virtual string? ForeignTableSchema { get; set; }

    /// <summary>
    /// Gets or sets the name of the primary (referenced) table in this foreign key relationship.
    /// </summary>
    public virtual string? PrimaryTable { get; set; }

    /// <summary>
    /// Gets or sets the schema name of the primary table in the foreign key relationship.
    /// </summary>
    public virtual string? PrimaryTableSchema { get; set; }

    /// <summary>
    /// Gets or sets the action to take when a referenced row is deleted.
    /// </summary>
    public virtual Rule OnDelete { get; set; }

    /// <summary>
    /// Gets or sets the action to take when the referenced key is updated.
    /// </summary>
    public virtual Rule OnUpdate { get; set; }

    /// <summary>
    /// Gets or sets the collection of column names in the foreign table that are part of the foreign key constraint.
    /// </summary>
    public virtual ICollection<string> ForeignColumns { get; set; }

    /// <summary>
    /// Gets or sets the collection of primary key column names involved in the foreign key.
    /// </summary>
    public virtual ICollection<string> PrimaryColumns { get; set; }
}
