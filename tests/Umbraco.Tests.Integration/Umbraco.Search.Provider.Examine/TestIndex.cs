using Examine.Lucene;
using Examine.Lucene.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine;

internal class TestIndex : LuceneIndex
{
    public TestIndex(ILoggerFactory loggerFactory, string name, IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions) : base(loggerFactory, name, indexOptions)
    {
    }
}
