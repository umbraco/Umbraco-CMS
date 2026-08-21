using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Declares which schema entity type a controller manages.
/// </summary>
/// <remarks>
/// Purely descriptive. Whether the entity type is actually governed is decided by configuration and by the
/// registered <see cref="ISchemaLockdownConfigurator"/> instances, never by this attribute.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class EntityTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityTypeAttribute"/> class.
    /// </summary>
    /// <param name="entityType">The entity type the controller manages.</param>
    public EntityTypeAttribute(string entityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        EntityType = entityType;
    }

    /// <summary>
    /// Gets the entity type the controller manages.
    /// </summary>
    public string EntityType { get; }
}
