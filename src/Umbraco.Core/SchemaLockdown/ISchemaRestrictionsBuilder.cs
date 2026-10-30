namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Builds the schema lockdown restrictions. Each <see cref="ISchemaLockdownConfigurator"/> receives it at start-up.
/// </summary>
/// <remarks>
/// Everything is permitted until a configurator denies it, and a denial can't be lifted, so the order configurators
/// run in doesn't matter. A configurator can read what earlier ones denied through <see cref="ISchemaRestrictions"/>.
/// </remarks>
public interface ISchemaRestrictionsBuilder : ISchemaRestrictions
{
    /// <summary>
    /// Denies the supplied operations on the supplied entity type.
    /// </summary>
    /// <remarks>
    /// Only <see cref="SchemaOperation.Create"/>, <see cref="SchemaOperation.Update"/> and
    /// <see cref="SchemaOperation.Delete"/> can be denied; any other operation is ignored.
    /// </remarks>
    /// <param name="entityType">The entity type, for example one of <see cref="Constants.UdiEntityType"/>.</param>
    /// <param name="operations">The operations to deny.</param>
    void Block(string entityType, params SchemaOperation[] operations);

    /// <summary>
    /// Denies creating, updating and deleting the supplied entity type.
    /// </summary>
    /// <param name="entityType">The entity type, for example one of <see cref="Constants.UdiEntityType"/>.</param>
    void BlockMutations(string entityType);
}
