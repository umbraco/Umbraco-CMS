using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Search.Core.Models.Indexing;

public record ContentChange
{
    private ContentChange(Guid id, UmbracoObjectTypes objectType, ChangeImpact changeImpact, ContentState contentState)
    {
        Id = id;
        ObjectType = objectType;
        ChangeImpact = changeImpact;
        ContentState = contentState;
    }

    public static ContentChange Document(Guid id, ChangeImpact changeImpact, ContentState contentState)
        => new (id, UmbracoObjectTypes.Document, changeImpact, contentState);

    public static ContentChange Media(Guid id, ChangeImpact changeImpact, ContentState contentState)
        => new (id, UmbracoObjectTypes.Media, changeImpact, contentState);

    public static ContentChange Member(Guid id, ChangeImpact changeImpact, ContentState contentState)
        => new (id, UmbracoObjectTypes.Member, changeImpact, contentState);

    public Guid Id { get; }

    public UmbracoObjectTypes ObjectType { get; }

    public ChangeImpact ChangeImpact { get; }

    public ContentState ContentState { get; }
}
