namespace Umbraco.Cms.Search.Provider.Examine.Services;

/// <summary>
/// A no-op implementation of <see cref="IActiveIndexManager"/> used when zero-downtime indexing is disabled.
/// Returns the index alias directly without any suffix, since only a single physical index is registered per alias.
/// </summary>
internal sealed class NoopActiveIndexManager : IActiveIndexManager
{
    /// <inheritdoc />
    public string ResolveActiveIndexName(string indexAlias) => indexAlias;

    /// <inheritdoc />
    public string ResolveShadowIndexName(string indexAlias) => indexAlias;

    /// <inheritdoc />
    public bool IsRebuilding(string indexAlias) => false;

    /// <inheritdoc />
    public void StartRebuilding(string indexAlias) { }

    /// <inheritdoc />
    public void CompleteRebuilding(string indexAlias) { }

    /// <inheritdoc />
    public void CancelRebuilding(string indexAlias) { }
}
