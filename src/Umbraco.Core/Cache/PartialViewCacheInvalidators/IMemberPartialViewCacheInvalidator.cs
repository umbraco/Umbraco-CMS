namespace Umbraco.Cms.Core.Cache.PartialViewCacheInvalidators;

public interface IMemberPartialViewCacheInvalidator
{
    void ClearPartialViewCacheItems(IEnumerable<int> updatedMemberIds);
}
