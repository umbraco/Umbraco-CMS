using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Models.Entities;

/// <summary>
/// Represents an entity that encapsulates information about a user's access rights or permissions within the Umbraco CMS API management models.
/// </summary>
public class UserAccessEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserAccessEntity"/> class with the specified entity and access flag.
    /// </summary>
    /// <param name="entity">The entity for which user access is being represented.</param>
    /// <param name="hasAccess">True if the user has access to the entity; otherwise, false.</param>
    public UserAccessEntity(IEntitySlim entity, bool hasAccess)
    {
        Entity = entity;
        HasAccess = hasAccess;
    }

    /// <summary>
    /// Gets the entity associated with the user access.
    /// </summary>
    public IEntitySlim Entity { get; }

    /// <summary>
    /// Gets a value indicating whether the user associated with this entity has access to the relevant resource or functionality.
    /// </summary>
    public bool HasAccess { get; }
}
