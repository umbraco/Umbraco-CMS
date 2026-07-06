using Examine.Lucene;
using Examine.Lucene.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Umbraco.Tests.Search.Examine.Integration;

internal class TestIndex : LuceneIndex
{
    public TestIndex(ILoggerFactory loggerFactory, string name, IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions) : base(loggerFactory, name, indexOptions)
    {
    }
}
