namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Represents a lightweight entity, managed by the entity service.
/// </summary>
public interface IEntitySlim : IUmbracoEntity, IHaveAdditionalData
{
    /// <summary>
    ///     Gets or sets the entity object type.
    /// </summary>
    Guid NodeObjectType { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the entity has children.
    /// </summary>
    bool HasChildren { get; }

    /// <summary>
    ///     Gets a value indicating whether the entity is a container.
    /// </summary>
    bool IsContainer { get; }
}
