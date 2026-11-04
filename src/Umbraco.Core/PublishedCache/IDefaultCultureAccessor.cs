namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
///     Gives access to the default culture.
/// </summary>
public interface IDefaultCultureAccessor
{
    /// <summary>
    ///     Gets the system default culture.
    /// </summary>
    /// <remarks>
    ///     <para>Implementations must NOT return a null value. Return an empty string for the invariant culture.</para>
    /// </remarks>
    string DefaultCulture { get; }

    /// <summary>
    ///     Discards any cached default culture, so the next read of <see cref="DefaultCulture" /> resolves it again.
    /// </summary>
    /// <remarks>Called when languages change. Implementations that don't cache can ignore it.</remarks>
    // TODO (V20): Remove the default implementation on this method.
    void ClearCache()
    {
    }
}
