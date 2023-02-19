namespace Umbraco.Search.Diagnostics;

/// <summary>
///     Default implementation of <see cref="IIndexDiagnosticsFactory" /> which returns
///     <see cref="GenericIndexDiagnostics" /> for indexes that don't have an implementation
/// </summary>
public class IndexDiagnosticsFactory : IIndexDiagnosticsFactory
{
    private readonly ISearchProvider _provider;

    public IndexDiagnosticsFactory(ISearchProvider provider)
    {
        _provider = provider;
    }

    public virtual IIndexDiagnostics Create(string index)
    {

        return new GenericIndexDiagnostics(_provider.GetIndex(index), _provider.GetSearcher(index));

    }

}
