namespace Umbraco.Cms.Api.Management.ViewModels;

/// <summary>
/// Marker interface that indicates the type can represent whether an entity has child entities.
/// </summary>
public interface IHasChildren
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity has at least one child entity of its own type.
    /// </summary>
    /// <remarks>
    /// Children are counted regardless of their published state and regardless of the requesting user's
    /// permissions, so an entity can report that it has children that the user is not permitted to see.
    /// </remarks>
    bool HasChildren { get; set; }
}
