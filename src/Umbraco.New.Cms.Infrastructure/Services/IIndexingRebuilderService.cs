namespace Umbraco.New.Cms.Infrastructure.Services;

public interface IIndexingRebuilderService
{
    void Set(string indexName);

    void Clear(string? indexName);

    bool Detect(string indexName);
}
