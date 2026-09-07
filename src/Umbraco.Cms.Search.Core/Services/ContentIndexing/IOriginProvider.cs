namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Supplies an identifier for the current server, used to filter out redundant same-origin indexing work in a load-balanced farm.
/// </summary>
public interface IOriginProvider
{
    /// <summary>
    /// Gets the identifier for the current server.
    /// </summary>
    /// <returns>The current server's origin identifier.</returns>
    string GetCurrent();
}
