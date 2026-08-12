namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Triggers an index rebuild across all servers in a load-balanced farm, via the distributed cache refresher infrastructure.
/// </summary>
public interface IDistributedContentIndexRebuilder
{
    /// <summary>
    /// Triggers a rebuild of the given index on every server.
    /// </summary>
    /// <param name="indexAlias">The alias of the index to rebuild.</param>
    /// <returns>True if a registration was found for the index and the rebuild was triggered; otherwise false.</returns>
    bool Rebuild(string indexAlias);
}
