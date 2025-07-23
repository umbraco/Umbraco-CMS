namespace Umbraco.Cms.Core.Cache.PartialViewCacheInvalidators;

/// <summary>
/// Provides a no-operation implementation of <see cref="IMemberPartialViewCacheInvalidator"/>.
/// </summary>
/// <remarks>
/// The default implementation is added in Umbraco.Web.Website, but we need this to ensure we have a service
/// registered for this interface even in headless setups).
/// </remarks>
internal sealed class NoopMemberPartialViewCacheInvalidator : IMemberPartialViewCacheInvalidator
{
    /// <inheritdoc/>
    public void ClearPartialViewCacheItems(IEnumerable<int> memberIds)
    {
        // No operation performed, this is a no-op implementation.
    }
}
