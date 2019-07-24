using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Store;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Examine;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

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

            //populate with some test data
            indexer.IndexItem(new ValueSet("1", "content", new Dictionary<string, object>
            {
                [fieldNames[0]] = "Hello world, there are products here",
                [UmbracoContentIndex.VariesByCultureFieldName] = "n"
            }));
            indexer.IndexItem(new ValueSet("2", "content", new Dictionary<string, object>
            {
                [fieldNames[1]] = "Hello world, there are products here",
                [UmbracoContentIndex.VariesByCultureFieldName] = "y"
            }));
            indexer.IndexItem(new ValueSet("3", "content", new Dictionary<string, object>
            {
                [fieldNames[2]] = "Hello world, there are products here",
                [UmbracoContentIndex.VariesByCultureFieldName] = "y"
            }));
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

        [Test]
        public void Search_Wildcard()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            {
                var fieldNames = new[] { "title", "title_en-us", "title_fr-fr" };
                using (var indexer = CreateTestIndex(luceneDir, fieldNames))
                {
                    var pcq = CreatePublishedContentQuery(indexer);

                    var results = pcq.Search("Products", "*", "TestIndex");

                    var ids = results.Select(x => x.Content.Id).ToList();
                    Assert.AreEqual(3, ids.Count);

                    //returns results for all fields and document types
                    Assert.IsTrue(ids.Contains(1) && ids.Contains(2) && ids.Contains(3));
                }
            }
        }

        [Test]
        public void Search_Invariant()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            {
                var fieldNames = new[] { "title", "title_en-us", "title_fr-fr" };
                using (var indexer = CreateTestIndex(luceneDir, fieldNames))
                {
                    var pcq = CreatePublishedContentQuery(indexer);

                    var results = pcq.Search("Products", null, "TestIndex");

                    var ids = results.Select(x => x.Content.Id).ToList();
                    Assert.AreEqual(1, ids.Count);

                    //returns results for only invariant fields and invariant documents
                    Assert.IsTrue(ids.Contains(1) && !ids.Contains(2) && !ids.Contains(3));
                }
            }
        }

        [Test]
        public void Search_Culture1()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            {
                var fieldNames = new[] { "title", "title_en-us", "title_fr-fr" };
                using (var indexer = CreateTestIndex(luceneDir, fieldNames))
                {
                    var pcq = CreatePublishedContentQuery(indexer);

                    var results = pcq.Search("Products", "en-us", "TestIndex");

                    var ids = results.Select(x => x.Content.Id).ToList();
                    Assert.AreEqual(2, ids.Count);

                    //returns results for en-us fields and invariant fields for all document types
                    Assert.IsTrue(ids.Contains(1) && ids.Contains(2) && !ids.Contains(3));
                }
            }
        }

        [Test]
        public void Search_Culture2()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            {
                var fieldNames = new[] { "title", "title_en-us", "title_fr-fr" };
                using (var indexer = CreateTestIndex(luceneDir, fieldNames))
                {
                    var pcq = CreatePublishedContentQuery(indexer);

                    var results = pcq.Search("Products", "fr-fr", "TestIndex");

                    var ids = results.Select(x => x.Content.Id).ToList();
                    Assert.AreEqual(2, ids.Count);

                    //returns results for fr-fr fields and invariant fields for all document types
                    Assert.IsTrue(ids.Contains(1) && !ids.Contains(2) && ids.Contains(3));
                }
            }
        }
    }
}
