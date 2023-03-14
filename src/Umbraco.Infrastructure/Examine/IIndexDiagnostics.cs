using Examine;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Exposes diagnostic information about an index
/// </summary>
public interface IIndexDiagnostics : IIndexStats
{
    /// <summary>
    ///     A key/value collection of diagnostic properties for the index
    /// </summary>
    /// <remarks>
    ///     Used to display in the UI
    /// </remarks>
    IReadOnlyDictionary<string, object?> Metadata { get; }

    /// <summary>
    ///     If the index can be open/read
    /// </summary>
    /// <returns>
    ///     A successful attempt if it is healthy, else a failed attempt with a message if unhealthy
    /// </returns>
    Attempt<string?> IsHealthy();
}
