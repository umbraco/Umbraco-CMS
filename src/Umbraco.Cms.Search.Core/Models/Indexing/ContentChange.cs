using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Search.Core.Models.Indexing;

/// <summary>
/// Represents a detected change to a content, media or member item that needs to be reflected in a search index.
/// </summary>
public record ContentChange
{
    private ContentChange(Guid id, UmbracoObjectTypes objectType, ChangeImpact changeImpact, ContentState contentState)
    {
        Id = id;
        ObjectType = objectType;
        ChangeImpact = changeImpact;
        ContentState = contentState;
    }

    /// <summary>
    /// Creates a <see cref="ContentChange"/> for a document.
    /// </summary>
    /// <param name="id">The key of the document.</param>
    /// <param name="changeImpact">The scope of re-indexing required.</param>
    /// <param name="contentState">The state (draft or published) the change applies to.</param>
    /// <returns>The created <see cref="ContentChange"/>.</returns>
    public static ContentChange Document(Guid id, ChangeImpact changeImpact, ContentState contentState)
        => new (id, UmbracoObjectTypes.Document, changeImpact, contentState);

    /// <summary>
    /// Creates a <see cref="ContentChange"/> for a media item.
    /// </summary>
    /// <param name="id">The key of the media item.</param>
    /// <param name="changeImpact">The scope of re-indexing required.</param>
    /// <param name="contentState">The state the change applies to.</param>
    /// <returns>The created <see cref="ContentChange"/>.</returns>
    public static ContentChange Media(Guid id, ChangeImpact changeImpact, ContentState contentState)
        => new (id, UmbracoObjectTypes.Media, changeImpact, contentState);

    /// <summary>
    /// Creates a <see cref="ContentChange"/> for a member.
    /// </summary>
    /// <param name="id">The key of the member.</param>
    /// <param name="changeImpact">The scope of re-indexing required.</param>
    /// <param name="contentState">The state the change applies to.</param>
    /// <returns>The created <see cref="ContentChange"/>.</returns>
    public static ContentChange Member(Guid id, ChangeImpact changeImpact, ContentState contentState)
        => new (id, UmbracoObjectTypes.Member, changeImpact, contentState);

    /// <summary>
    /// Gets the key of the changed item.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the entity type of the changed item.
    /// </summary>
    public UmbracoObjectTypes ObjectType { get; }

    /// <summary>
    /// Gets the scope of re-indexing this change requires.
    /// </summary>
    public ChangeImpact ChangeImpact { get; }

    /// <summary>
    /// Gets the state (draft or published) this change applies to.
    /// </summary>
    public ContentState ContentState { get; }
}
