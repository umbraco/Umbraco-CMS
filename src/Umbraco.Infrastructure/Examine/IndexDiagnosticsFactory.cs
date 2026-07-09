using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Default implementation of <see cref="IIndexDiagnosticsFactory" /> which returns
///     <see cref="GenericIndexDiagnostics" /> for indexes that don't have an implementation
/// </summary>
public class IndexDiagnosticsFactory : IIndexDiagnosticsFactory
{
    /// <summary>
    /// Creates an <see cref="IIndexDiagnostics"/> instance for the given <see cref="IIndex"/>.
    /// </summary>
    /// <param name="index">The index to create diagnostics for.</param>
    /// <returns>An <see cref="IIndexDiagnostics"/> instance associated with the provided index.</returns>
    public virtual IIndexDiagnostics Create(IIndex index)
    {
        if (index is not IIndexDiagnostics indexDiag)
        {
            indexDiag = new GenericIndexDiagnostics(index);
        }

        return indexDiag;
    }
}
