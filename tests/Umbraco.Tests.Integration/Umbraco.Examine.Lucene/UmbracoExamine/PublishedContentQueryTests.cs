using Examine;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Testing;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
public class PublishedContentQueryTests : ExamineBaseTest
{
    private class TestIndex : LuceneIndex, IUmbracoIndex
    {
        private readonly string[] _fieldNames;

        public TestIndex(ILoggerFactory loggerFactory, string name, Directory luceneDirectory, string[] fieldNames)
            : base(
                loggerFactory,
                name,
                IndexInitializer.GetOptions(
                    name,
                    new LuceneDirectoryIndexOptions
                    {
                        DirectoryFactory = new GenericDirectoryFactory(s => luceneDirectory)
                    })) =>
            _fieldNames = fieldNames;

        public bool EnableDefaultEventHandler => throw new NotImplementedException();
        public bool PublishedValuesOnly => throw new NotImplementedException();
        public bool SupportProtectedContent => throw new NotImplementedException();
        public IEnumerable<string> GetFields() => _fieldNames;
    }

    private TestIndex CreateTestIndex(Directory luceneDirectory, (string Name, string Culture)[] fields)
    {
        var index = new TestIndex(LoggerFactory, "TestIndex", luceneDirectory, fields.Select(f => f.Name).ToArray());

        using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
        {
            // populate with some test data
            for (var i = 0; i < fields.Length; i++)
            {
                var (name, culture) = fields[i];

                index.IndexItem(new ValueSet(
                    $"{i + 1}",
                    "content",
                    new Dictionary<string, object>
                    {
                        [name] = "Hello world, there are products here",
                        [UmbracoExamineFieldNames.VariesByCultureFieldName] = culture.IsNullOrEmpty() ? "n" : "y",
                        [culture.IsNullOrEmpty() ? UmbracoExamineFieldNames.PublishedFieldName : $"{UmbracoExamineFieldNames.PublishedFieldName}_{culture}"] = "y"
                    }));
            }
        }

        return index;
    }

    private PublishedContentQuery CreatePublishedContentQuery(IIndex indexer)
    {
        var examineManager = new Mock<IExamineManager>();
        var outarg = indexer;
        examineManager.Setup(x => x.TryGetIndex("TestIndex", out outarg)).Returns(true);

        var contentCache = new Mock<IPublishedContentCache>();
        contentCache.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int intId) => Mock.Of<IPublishedContent>(x => x.Id == intId));
        var snapshot = Mock.Of<IPublishedSnapshot>(x => x.Content == contentCache.Object);
        var variationContext = new VariationContext();
        var variationContextAccessor = Mock.Of<IVariationContextAccessor>(x => x.VariationContext == variationContext);

        return new PublishedContentQuery(snapshot, variationContextAccessor, examineManager.Object);
    }

    [TestCase("fr-fr", ExpectedResult = "1, 3", Description = "Search Culture: fr-fr. Must return both fr-fr and invariant results")]
    [TestCase("en-us", ExpectedResult = "1, 2", Description = "Search Culture: en-us. Must return both en-us and invariant results")]
    [TestCase("*", ExpectedResult = "1, 2, 3", Description = "Search Culture: *. Must return all cultures and all invariant results")]
    [TestCase(null, ExpectedResult = "1", Description = "Search Culture: null. Must return only invariant results")]
    [LongRunning]
    public string Search(string culture)
    {
        using (var luceneDir = new RandomIdRAMDirectory())
        {
            var fields = new[] { (Name: "title", Culture: null), (Name: "title_en-us", Culture: "en-us"), (Name: "title_fr-fr", Culture: "fr-fr") };
            using (var indexer = CreateTestIndex(luceneDir, fields))
            {
                var pcq = CreatePublishedContentQuery(indexer);

                var results = pcq.Search("Products", culture, "TestIndex");

                var ids = results.Select(x => x.Content.Id).ToArray();

                return string.Join(", ", ids);
            }
        }
    }
}
