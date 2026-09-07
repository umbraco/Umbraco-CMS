using Examine;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Provider.Examine.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;

public abstract class IndexTestBase : TestBase
{
    protected IExamineManager ExamineManager => GetRequiredService<IExamineManager>();
    protected IActiveIndexManager ActiveIndexManager => GetRequiredService<IActiveIndexManager>();
    protected IIndexer Indexer => GetRequiredService<IIndexer>();

    protected IIndex GetIndex(string indexAlias)
    {
        var physicalName = ActiveIndexManager.ResolveActiveIndexName(indexAlias);
        return ExamineManager.GetIndex(physicalName);
    }
}
