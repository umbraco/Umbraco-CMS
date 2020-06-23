using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.Search;
using NUnit.Framework;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Services;
using Umbraco.Tests.Testing;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Composing;
using Umbraco.Examine;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class SearchTests : ExamineBaseTest
    {

        [Test]
        public void Test_Sort_Order_Sorting()
        {
            long totalRecs;
            var demoData = new ExamineDemoDataContentService(TestFiles.umbraco_sort);
            var allRecs = demoData.GetLatestContentByXPath("//*[@isDoc]")
                .Root
                .Elements()
                .Select(x => Mock.Of<IContent>(
                    m =>
                        m.Id == (int)x.Attribute("id") &&
                        m.ParentId == (int)x.Attribute("parentID") &&
                        m.Level == (int)x.Attribute("level") &&
                        m.CreatorId == 0 &&
                        m.SortOrder == (int)x.Attribute("sortOrder") &&
                        m.CreateDate == (DateTime)x.Attribute("createDate") &&
                        m.UpdateDate == (DateTime)x.Attribute("updateDate") &&
                        m.Name == (string)x.Attribute("nodeName") &&
                        m.GetCultureName(It.IsAny<string>()) == (string)x.Attribute("nodeName") &&
                        m.Path == (string)x.Attribute("path") &&
                        m.Properties == new PropertyCollection() &&
                        m.Published == true &&
                        m.ContentType == Mock.Of<ISimpleContentType>(mt =>
                            mt.Icon == "test" &&
                            mt.Alias == x.Name.LocalName &&
                            mt.Id == (int)x.Attribute("nodeType"))))
                .ToArray();
            var contentService = Mock.Of<IContentService>(
                x => x.GetPagedDescendants(
                    It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out totalRecs, It.IsAny<IQuery<IContent>>(), It.IsAny<Ordering>())
                    ==
                    allRecs);

            var propertyEditors = Factory.GetInstance<PropertyEditorCollection>();
            var rebuilder = IndexInitializer.GetContentIndexRebuilder(propertyEditors, contentService, ScopeProvider, true);

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir))
            using (indexer.ProcessNonAsync())
            {
                indexer.CreateIndex();
                rebuilder.Populate(indexer);

                var searcher = indexer.GetSearcher();

                var numberSortedCriteria = searcher.CreateQuery()
                    .ParentId(1148)
                    .OrderBy(new SortableField("sortOrder", SortType.Int));
                var numberSortedResult = numberSortedCriteria.Execute();

                var stringSortedCriteria = searcher.CreateQuery()
                    .ParentId(1148)
                    .OrderBy(new SortableField("sortOrder"));//will default to string
                var stringSortedResult = stringSortedCriteria.Execute();

                Assert.AreEqual(12, numberSortedResult.TotalItemCount);
                Assert.AreEqual(12, stringSortedResult.TotalItemCount);

                Assert.IsTrue(IsSortedByNumber(numberSortedResult));
                Assert.IsFalse(IsSortedByNumber(stringSortedResult));
            }
        }

        private bool IsSortedByNumber(IEnumerable<ISearchResult> results)
        {
            var currentSort = 0;
            foreach (var searchResult in results)
            {
                var sort = int.Parse(searchResult.Values["sortOrder"]);
                if (currentSort >= sort)
                {
                    return false;
                }
                currentSort = sort;
            }
            return true;
        }

        //[Test]
        //public void Test_Index_Type_With_German_Analyzer()
        //{
        //    using (var luceneDir = new RandomIdRamDirectory())
        //    {
        //        var indexer = IndexInitializer.GetUmbracoIndexer(luceneDir,
        //            new GermanAnalyzer());
        //        indexer.RebuildIndex();
        //        var searcher = IndexInitializer.GetUmbracoSearcher(luceneDir);
        //    }
        //}

        //private readonly TestContentService _contentService = new TestContentService();
        //private readonly TestMediaService _mediaService = new TestMediaService();
        //private static UmbracoExamineSearcher _searcher;
        //private static UmbracoContentIndexer _indexer;
        //private Lucene.Net.Store.Directory _luceneDir;

    }
}
