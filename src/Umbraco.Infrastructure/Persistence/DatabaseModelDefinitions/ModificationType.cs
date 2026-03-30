namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
/// Specifies the types of modifications that can be performed on a database model definition.
/// </summary>
public enum ModificationType
{
    Create,
    Alter,
    Drop,
    Rename,
    Insert,
    Update,
    Delete,
}
