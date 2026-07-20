using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Notifications;

public sealed class ContentIndexingNotification : ICancelableNotification
{
    public ContentIndexingNotification(
        string indexAlias,
        Guid id,
        UmbracoObjectTypes objectType,
        IEnumerable<Variation> variations,
        IEnumerable<IndexField> fields)
    {
        IndexAlias = indexAlias;
        Id = id;
        ObjectType = objectType;
        Variations = variations;
        Fields = fields;
    }

    public string IndexAlias { get; }

    public Guid Id { get; }

    public UmbracoObjectTypes ObjectType { get; }

    public IEnumerable<Variation> Variations { get; }

    public IEnumerable<IndexField> Fields { get; set; }

    public bool Cancel { get; set; }
}
