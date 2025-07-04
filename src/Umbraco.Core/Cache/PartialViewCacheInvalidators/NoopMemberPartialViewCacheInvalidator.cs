namespace Umbraco.Cms.Core.Cache.PartialViewCacheInvalidators;

internal class NoopMemberPartialViewCacheInvalidator : IMemberPartialViewCacheInvalidator
{
    public void ClearPartialViewCacheItems(IEnumerable<int> memberIds)
    {
        // No operation performed, this is a no-op implementation.
    }
}
