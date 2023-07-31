using Umbraco.Cms.Core.Notifications;
using Umbraco.Search.ValueSet;

namespace Umbraco.Search.Indexing.Notifications;

public class IndexingNotification : INotification
{
    public IEnumerable<UmbracoValueSet> Values { get; }

    public IndexingNotification(IEnumerable<UmbracoValueSet> values)
    {
        Values = values;
    }
}
public class RemoveFromIndexNotification : INotification
{
    public RemoveFromIndexNotification(IEnumerable<string> ids)
    {
        Ids = ids;
    }

    public IEnumerable<string> Ids { get; set; }
}
