namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

public interface IDistributedContentIndexRebuilder
{
    bool Rebuild(string indexAlias);
}
