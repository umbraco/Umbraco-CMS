namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

public class ConstraintDefinition
{
    private readonly ConstraintType _constraintType;
    public ICollection<string?> Columns = new HashSet<string?>();

    public ConstraintDefinition(ConstraintType type) => _constraintType = type;

    public bool IsPrimaryKeyConstraint => ConstraintType.PrimaryKey == _constraintType;
    public bool IsUniqueConstraint => ConstraintType.Unique == _constraintType;
    public bool IsNonUniqueConstraint => ConstraintType.NonUnique == _constraintType;

    public string? SchemaName { get; set; }
    public string? ConstraintName { get; set; }
    public string? TableName { get; set; }
    public bool IsPrimaryKeyClustered { get; set; }
}
