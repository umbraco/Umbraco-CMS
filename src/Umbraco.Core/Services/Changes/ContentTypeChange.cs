using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Changes;

public class ContentTypeChange<TItem>
    where TItem : class, IContentTypeComposition
{
    public ContentTypeChange(TItem item, ContentTypeChangeTypes changeTypes)
    {
        Item = item;
        ChangeTypes = changeTypes;
    }

    public TItem Item { get; }

    public ContentTypeChangeTypes ChangeTypes { get; set; }
}
