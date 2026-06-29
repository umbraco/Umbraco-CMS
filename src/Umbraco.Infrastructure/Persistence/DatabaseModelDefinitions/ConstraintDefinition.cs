namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
/// Represents the definition of a database constraint, such as a primary key, foreign key, or unique constraint, used within the persistence layer.
/// </summary>
public class ConstraintDefinition
{
    public ICollection<string?> Columns = new HashSet<string?>();
    private readonly ConstraintType _constraintType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintDefinition"/> class using the specified constraint type.
    /// </summary>
    /// <param name="type">The constraint type.</param>
    public ConstraintDefinition(ConstraintType type) => _constraintType = type;

    /// <summary>
    /// Gets a value indicating whether this constraint is a primary key constraint.
    /// </summary>
    public bool IsPrimaryKeyConstraint => _constraintType == ConstraintType.PrimaryKey;

    /// <summary>
    /// Gets a value indicating whether this constraint is a unique constraint.
    /// </summary>
    public bool IsUniqueConstraint => _constraintType == ConstraintType.Unique;

    /// <summary>
    /// Gets a value indicating whether this constraint is non-unique.
    /// </summary>
    public bool IsNonUniqueConstraint => _constraintType == ConstraintType.NonUnique;

    /// <summary>
    /// Gets or sets the schema name associated with the constraint.
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// Gets or sets the name of the database constraint associated with this definition.
    /// </summary>
    public string? ConstraintName { get; set; }

    /// <summary>
    /// Gets or sets the name of the table associated with the constraint.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the primary key constraint is clustered on the table.
    /// If <c>true</c>, the primary key is implemented as a clustered index; otherwise, it is non-clustered.
    /// </summary>
    public bool IsPrimaryKeyClustered { get; set; }
}
