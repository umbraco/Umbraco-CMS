using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Creates <see cref="IIndexDiagnostics" /> for an index if it doesn't implement <see cref="IIndexDiagnostics" />
/// </summary>
public interface IIndexDiagnosticsFactory
{
    /// <summary>
    /// Creates an <see cref="IIndexDiagnostics"/> instance for the specified index.
    /// </summary>
    /// <param name="index">The index to create diagnostics for.</param>
    /// <returns>An <see cref="IIndexDiagnostics"/> instance.</returns>
    IIndexDiagnostics Create(IIndex index);
}
