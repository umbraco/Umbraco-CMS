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
    /// Clears the cache.
    /// </summary>
    // TODO (V20): Remove the default implementation on this method.
    void ClearCache()
    {
    }
}
