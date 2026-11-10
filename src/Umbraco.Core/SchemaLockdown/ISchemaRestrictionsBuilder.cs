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
    /// Denies the supplied permission verbs.
    /// </summary>
    /// <remarks>
    /// A denial is enforced on the Management API endpoints that change the entity types they declare. Schema lockdown
    /// governs only changes; reads are always permitted.
    /// </remarks>
    /// <param name="permissions">The permission verbs, for example one of <see cref="Constants.Permissions"/>. Matched case-sensitively.</param>
    /// <exception cref="ArgumentException">A permission verb is empty.</exception>
    void Deny(params string[] permissions);
}
