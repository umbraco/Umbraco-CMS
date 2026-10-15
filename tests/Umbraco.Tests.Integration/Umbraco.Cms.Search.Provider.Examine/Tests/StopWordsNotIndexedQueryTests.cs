using Examine.Lucene;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Search.Querying;
using Umbraco.Cms.Tests.Common.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests;

// an index whose analyzer drops common words, as indexes built with the default analyzer do
[LongRunning]
public class StopWordsNotIndexedQueryTests : SearcherTestBase
{
    protected override void ConfigureServices(IServiceCollection services)
        => services.PostConfigureAll<LuceneDirectoryIndexOptions>(
            options => options.Analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48));

    [TestCase("probe for the member")]
    [TestCase("Probe For The Member")]
    [TestCase("probe the")]
    public async Task CanQueryWithCommonWordsNotInIndex(string query)
    {
        SearchResult result = await SearchAsync(query: query);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[StopWordsDocumentNumber]));
            });
    }

    [Test]
    public async Task CommonWordsDoNotRelaxOtherQueryTerms()
    {
        SearchResult result = await SearchAsync(query: "even the");

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(50));
                Assert.That(result.Documents.Select(document => document.Id), Is.EquivalentTo(OddOrEvenIds(true).Select(i => DocumentIds[i])));
            });
    }
}
