namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides additional cache tags to evict from the Delivery API output cache when content changes.
/// </summary>
/// <remarks>
///     <para>
///         Multiple implementations can be registered. The eviction handler iterates all providers
///         to collect additional tags to evict beyond the built-in content key tag.
///     </para>
///     <para>
///         Works as a pair with <see cref="IDeliveryApiOutputCacheTagProvider"/>: the tag provider adds
///         custom tags when caching a response, and this provider maps content changes back to those
///         tags at eviction time.
///     </para>
/// </remarks>
public interface IDeliveryApiOutputCacheEvictionProvider
{
    /// <summary>
    ///     Returns additional cache tags to evict when the specified content changes.
    /// </summary>
    /// <param name="context">Details of the content change.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Additional cache tags to evict.</returns>
    Task<IEnumerable<string>> GetAdditionalEvictionTagsAsync(OutputCacheContentChangedContext context, CancellationToken cancellationToken = default);
}
