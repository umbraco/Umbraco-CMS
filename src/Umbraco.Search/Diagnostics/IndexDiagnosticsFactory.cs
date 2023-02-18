namespace Umbraco.Search.Diagnostics;

/// <summary>
///     Default implementation of <see cref="IIndexDiagnosticsFactory" /> which returns
///     <see cref="GenericIndexDiagnostics" /> for indexes that don't have an implementation
/// </summary>
public class IndexDiagnosticsFactory<T>: IIndexDiagnosticsFactory<T>
{
    private readonly ISearchProvider _provider;

    public IndexDiagnosticsFactory(ISearchProvider provider)
    {
        _provider = provider;
    }

    public virtual IIndexDiagnostics<T> Create<T>(string index)
    {
        if (index is not IIndexDiagnostics<T> indexDiag)
        {
            indexDiag = new GenericIndexDiagnostics<T>(_provider.GetIndex<T>(index), _provider.GetSearcher<T>(index));
        }

        return indexDiag;
    }

}
