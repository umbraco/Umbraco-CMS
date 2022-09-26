using Examine;

namespace Umbraco.New.Cms.Infrastructure.Services;

public interface IIndexingRebuilderService
{
    bool Rebuild(IIndex index, string indexName);
    void Set(string indexName);

    void Clear(string? indexName);

    bool Detect(string indexName);
}
