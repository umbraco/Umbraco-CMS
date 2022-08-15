namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

public class ConstraintDefinition
{
    public ICollection<string?> Columns = new HashSet<string?>();
    private readonly ConstraintType _constraintType;

    public ConstraintDefinition(ConstraintType type) => _constraintType = type;

    public bool IsPrimaryKeyConstraint => _constraintType == ConstraintType.PrimaryKey;

    public bool IsUniqueConstraint => _constraintType == ConstraintType.Unique;

    public bool IsNonUniqueConstraint => _constraintType == ConstraintType.NonUnique;

    public string? SchemaName { get; set; }

    public string? ConstraintName { get; set; }

    public string? TableName { get; set; }

    public bool IsPrimaryKeyClustered { get; set; }
}
