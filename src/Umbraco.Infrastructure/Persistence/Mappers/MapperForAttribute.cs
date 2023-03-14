namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     An attribute used to decorate mappers to be associated with entities
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class MapperForAttribute : Attribute
{
    public MapperForAttribute(Type entityType) => EntityType = entityType;

    public Type EntityType { get; }
}
