namespace Umbraco.Search.Services;

/// <summary>
///    A Marker interface for defining an  handler to maintain the main dom for search providers
/// </summary>
public interface ISearchMainDomHandler
{
    public bool IsMainDom();
}
