namespace Umbraco.Cms.Search.Provider.Examine.Services;

/// <summary>
/// A no-op implementation of <see cref="IActiveIndexManager"/> used when zero-downtime indexing is disabled.
/// Returns the index alias directly without any suffix, since only a single physical index is registered per alias.
/// </summary>
internal sealed class NoopActiveIndexManager : IActiveIndexManager
{
    public string ResolveActiveIndexName(string indexAlias) => indexAlias;

    public string ResolveShadowIndexName(string indexAlias) => indexAlias;

    public bool IsRebuilding(string indexAlias) => false;

    public void StartRebuilding(string indexAlias) { }

    public void CompleteRebuilding(string indexAlias) { }

    public void CancelRebuilding(string indexAlias) { }
}
