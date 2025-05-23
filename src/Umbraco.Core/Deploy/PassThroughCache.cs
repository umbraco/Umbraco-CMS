namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// A pass through context cache that always creates the items.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.Deploy.IContextCache" />
public sealed class PassThroughCache : IContextCache
{
    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>
    /// The instance.
    /// </value>
    public static PassThroughCache Instance { get; } = new PassThroughCache();

    /// <summary>
    /// Prevents a default instance of the <see cref="PassThroughCache"/> class from being created.
    /// </summary>
    private PassThroughCache()
    { }

    /// <inheritdoc />
    public void Create<T>(string key, T item)
    { }

    /// <inheritdoc />
    public T? GetOrCreate<T>(string key, Func<T?> factory)
        => factory();

    /// <inheritdoc />
    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory)
        => await factory().ConfigureAwait(false);

    /// <inheritdoc />
    public void Clear()
    { }
}
