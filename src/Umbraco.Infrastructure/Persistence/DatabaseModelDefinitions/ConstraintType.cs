namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
/// Specifies the various types of constraints that can be applied to a database table, such as primary keys, foreign keys, and unique constraints.
/// </summary>
public enum ConstraintType
{
    PrimaryKey,
    Unique,
    NonUnique,
}
