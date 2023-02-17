using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Default implementation of <see cref="IIndexDiagnosticsFactory" /> which returns
///     <see cref="GenericIndexDiagnostics" /> for indexes that don't have an implementation
/// </summary>
public class IndexDiagnosticsFactory : IIndexDiagnosticsFactory
{
    public virtual IIndexDiagnostics Create(IIndex index)
    {
        if (index is not IIndexDiagnostics indexDiag)
        {
            indexDiag = new GenericIndexDiagnostics(index);
        }

        return indexDiag;
    }
}
