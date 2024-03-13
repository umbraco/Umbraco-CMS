using Examine;

namespace Umbraco.Cms.Infrastructure.Examine.Deferred;

internal abstract class DeliveryApiContentIndexDeferredBase
{
    protected static void RemoveFromIndex(int id, IIndex index)
        => RemoveFromIndex(new[] { id }, index);

    protected static void RemoveFromIndex(IReadOnlyCollection<int> ids, IIndex index)
        => RemoveFromIndex(ids.Select(id => id.ToString()).ToArray(), index);

    protected static void RemoveFromIndex(IReadOnlyCollection<string> ids, IIndex index)
    {
        if (ids.Any() is false)
        {
            return;
        }

        // NOTE: the delivery api index implementation takes care of deleting descendants, so we don't have to do that here
        index.DeleteFromIndex(ids);
    }
}
