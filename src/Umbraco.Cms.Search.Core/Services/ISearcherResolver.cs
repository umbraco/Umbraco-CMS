namespace Umbraco.Cms.Search.Core.Services;

public interface ISearcherResolver
{
    public ISearcher? GetSearcher(string indexAlias);
}
