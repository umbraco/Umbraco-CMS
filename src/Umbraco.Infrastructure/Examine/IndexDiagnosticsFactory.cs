using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Default implementation of <see cref="IIndexDiagnosticsFactory" /> which returns
///     <see cref="GenericIndexDiagnostics" /> for indexes that don't have an implementation
/// </summary>
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]

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
