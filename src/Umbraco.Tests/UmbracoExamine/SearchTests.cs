using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Store;
using NUnit.Framework;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.UmbracoExamine
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class SearchTests : ExamineBaseTest
    {

        [Test]
        public void Test_Sort_Order_Sorting()
        {
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var writer = new IndexWriter(luceneDir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), IndexWriter.MaxFieldLength.LIMITED))
            using (var indexer = IndexInitializer.GetUmbracoIndexer(writer, null,
                    new TestDataService()
                    {
                        ContentService = new TestContentService(TestFiles.umbraco_sort)
                    },
                    supportUnpublishedContent: true))
            using (var searcher = IndexInitializer.GetUmbracoSearcher(writer))
            {                
                indexer.RebuildIndex();                

                var s = (LuceneSearcher)searcher;
                var luceneSearcher = s.GetSearcher();
                var i = (LuceneIndexer)indexer;

                var numberSortedCriteria = searcher.CreateSearchCriteria()
                    .ParentId(1148).And()
                    .OrderBy(new SortableField("sortOrder", SortType.Int));
                var numberSortedResult = searcher.Search(numberSortedCriteria.Compile());

                var stringSortedCriteria = searcher.CreateSearchCriteria()
                    .ParentId(1148).And()
                    .OrderBy("sortOrder"); //will default to string
                var stringSortedResult = searcher.Search(stringSortedCriteria.Compile());

                Assert.AreEqual(12, numberSortedResult.TotalItemCount);
                Assert.AreEqual(12, stringSortedResult.TotalItemCount);

                Assert.IsTrue(IsSortedByNumber(numberSortedResult));
                Assert.IsFalse(IsSortedByNumber(stringSortedResult));
            }
        }

        private bool IsSortedByNumber(IEnumerable<SearchResult> results)
        {
            var currentSort = 0;
            foreach (var searchResult in results)
            {
                var sort = int.Parse(searchResult.Fields["sortOrder"]);
                if (currentSort >= sort)
                {
                    return false;
                }
                currentSort = sort;
            }
            return true;
        }        

    }
}
