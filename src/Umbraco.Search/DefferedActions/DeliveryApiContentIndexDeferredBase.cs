using Umbraco.Cms.Core.Models;

namespace Umbraco.Search.DefferedActions;

internal abstract class DeliveryApiContentIndexDeferredBase
{
    private readonly ISearchProvider _provider;

    protected DeliveryApiContentIndexDeferredBase(ISearchProvider provider)
    {
        _provider = provider;
    }

    protected void RemoveFromIndex(int id, string index)
        => RemoveFromIndex(new[] { id }, index);

    protected void RemoveFromIndex(IReadOnlyCollection<int> ids, string index)
        => RemoveFromIndex(ids.Select(id => id.ToString()).ToArray(), index);

    protected void RemoveFromIndex(IReadOnlyCollection<string> ids, string index)
    {
        if (ids.Any() is false)
        {
            return;
        }

        // NOTE: the delivery api index implementation takes care of deleting descendants, so we don't have to do that here
        _provider.GetIndex<IContent>(index)?.RemoveFromIndex(ids.Select(id => id.ToString()));
    }
}
