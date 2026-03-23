namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     An attribute used to decorate mappers to be associated with entities
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class MapperForAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapperForAttribute"/> class, specifying the entity type to map.
    /// </summary>
    /// <param name="entityType">The entity type that this attribute applies to.</param>
    public MapperForAttribute(Type entityType) => EntityType = entityType;

    /// <summary>
    /// Gets the type of the entity that this mapper is associated with.
    /// </summary>
    public Type EntityType { get; }
}
