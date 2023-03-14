namespace Umbraco.Cms.Core.Scoping;

/// <summary>
///     Specifies the cache mode of repositories.
/// </summary>
public enum RepositoryCacheMode
{
    /// <summary>
    ///     Unspecified.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    ///     Default, full L2 cache.
    /// </summary>
    Default = 1,

    /// <summary>
    ///     Scoped cache.
    /// </summary>
    /// <remarks>
    ///     <para>Reads from, and writes to, a scope-local cache.</para>
    ///     <para>Upon scope completion, clears the global L2 cache.</para>
    /// </remarks>
    Scoped = 2,

    /// <summary>
    ///     No cache.
    /// </summary>
    /// <remarks>
    ///     <para>Bypasses caches entirely.</para>
    ///     <para>Upon scope completion, clears the global L2 cache.</para>
    /// </remarks>
    None = 3,
}
