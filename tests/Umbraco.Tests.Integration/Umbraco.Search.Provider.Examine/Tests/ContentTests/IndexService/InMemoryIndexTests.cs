using Examine;
using Examine.Lucene;
using Examine.Lucene.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
public class InMemoryIndexTests : UmbracoIntegrationTest
{
    private IExamineManager ExamineManager => GetRequiredService<IExamineManager>();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddExamine();
        services.AddSingleton<TestInMemoryDirectoryFactory>();
        services.AddExamineLuceneIndex<TestIndex, TestInMemoryDirectoryFactory>(TestIndex.TestIndexName);
    }

    private class TestIndex : LuceneIndex
    {
        public const string TestIndexName = "TestIndex";

        public TestIndex(ILoggerFactory loggerFactory, string name, IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions) : base(loggerFactory, name, indexOptions)
        {
        }
    }

    [Test]
    public void CanIndexAnyData()
    {
        IIndex index = GetIndex();
        index.IndexItem(new ValueSet(
            "test",
            "Person",
            new Dictionary<string, object>()
            {
                {"FirstName", "Nikolaj" },
                {"LastName", "Geisle" },
                {"Email", "nge@umbraco.dk" },
                {"Age", 30}
            }));

        Thread.Sleep(3000);

        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results.TotalItemCount, Is.EqualTo(1));
        Assert.That(results.First().Id, Is.EqualTo("test"));
    }

    [Test]
    [TestCase(3)]
    [TestCase(50)]
    [TestCase(100)]
    public void CanIndexData(int count)
    {
        IIndex index = GetIndex();
        IndexData(index, count);
        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results.TotalItemCount, Is.EqualTo(count));
    }

    private void IndexData(IIndex index, int count = 3)
    {
        for (int i = 0; i < count; i++)
        {
            index.IndexItem(new ValueSet(
                $"TestId{i}",
                "Person",
                new Dictionary<string, object>()
                {
                    {"FirstName", $"FirstName{i}" },
                    {"LastName",  $"LastName{i}" },
                    {"Age", i },
                }));
        }

        Thread.Sleep(3000);
    }

    private IIndex GetIndex()
    {
        ExamineManager.TryGetIndex("TestIndex", out IIndex? index);
        return index!;
    }
}
