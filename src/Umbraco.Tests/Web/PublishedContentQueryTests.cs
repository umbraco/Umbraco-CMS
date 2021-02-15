using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Store;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests.Web
{
    [TestFixture]
    public class PublishedContentQueryTests
    {

        private class TestIndex : LuceneIndex, IUmbracoIndex
        {
            private readonly string[] _fieldNames;

            public TestIndex(string name, Directory luceneDirectory, string[] fieldNames)
                : base(name, luceneDirectory, null, null, null, null)
            {
                _fieldNames = fieldNames;
            }
            public bool EnableDefaultEventHandler => throw new NotImplementedException();
            public bool PublishedValuesOnly => throw new NotImplementedException();
            public IEnumerable<string> GetFields() => _fieldNames;
        }

        private TestIndex CreateTestIndex(Directory luceneDirectory, string[] fieldNames)
        {
            var indexer = new TestIndex("TestIndex", luceneDirectory, fieldNames);

            using (indexer.ProcessNonAsync())
            {
                //populate with some test data
                indexer.IndexItem(new ValueSet("1", "content", new Dictionary<string, object>
                {
                    [fieldNames[0]] = "Hello world, there are products here",
                    [UmbracoExamineFieldNames.VariesByCultureFieldName] = "n"
                }));
                indexer.IndexItem(new ValueSet("2", "content", new Dictionary<string, object>
                {
                    [fieldNames[1]] = "Hello world, there are products here",
                    [UmbracoExamineFieldNames.VariesByCultureFieldName] = "y"
                }));
                indexer.IndexItem(new ValueSet("3", "content", new Dictionary<string, object>
                {
                    [fieldNames[2]] = "Hello world, there are products here",
                    [UmbracoExamineFieldNames.VariesByCultureFieldName] = "y"
                }));
            }

            return indexer;
        }

        private PublishedContentQuery CreatePublishedContentQuery(IIndex indexer)
        {
            var examineManager = new Mock<IExamineManager>();
            IIndex outarg = indexer;
            examineManager.Setup(x => x.TryGetIndex("TestIndex", out outarg)).Returns(true);

            var contentCache = new Mock<IPublishedContentCache>();
            contentCache.Setup(x => x.GetById(It.IsAny<int>())).Returns((int intId) => Mock.Of<IPublishedContent>(x => x.Id == intId));
            var snapshot = Mock.Of<IPublishedSnapshot>(x => x.Content == contentCache.Object);
            var variationContext = new VariationContext();
            var variationContextAccessor = Mock.Of<IVariationContextAccessor>(x => x.VariationContext == variationContext);

            return new PublishedContentQuery(snapshot, variationContextAccessor, examineManager.Object);
        }

        [TestCase("fr-fr", ExpectedResult = "1, 3", TestName = "Search Culture: fr-fr. Must return both fr-fr and invariant results")]
        [TestCase("en-us", ExpectedResult = "1, 2", TestName = "Search Culture: en-us. Must return both en-us and invariant results")]
        [TestCase("*", ExpectedResult = "1, 2, 3", TestName = "Search Culture: *. Must return all cultures and all invariant results")]
        [TestCase(null, ExpectedResult = "1", TestName = "Search Culture: null. Must return only invariant results")]
        public string Search(string culture)
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            {
                var fieldNames = new[] { "title", "title_en-us", "title_fr-fr" };
                using (var indexer = CreateTestIndex(luceneDir, fieldNames))
                {
                    var pcq = CreatePublishedContentQuery(indexer);

                    var results = pcq.Search("Products", culture, "TestIndex");

                    var ids = results.Select(x => x.Content.Id).ToArray();

                    return string.Join(", ", ids);
                }
            }
        }
    }
}
