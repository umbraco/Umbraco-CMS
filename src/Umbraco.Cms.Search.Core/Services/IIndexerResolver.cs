namespace Umbraco.Cms.Search.Core.Services;

public interface IIndexerResolver
{
    public IIndexer? GetIndexer(string indexAlias);
}
