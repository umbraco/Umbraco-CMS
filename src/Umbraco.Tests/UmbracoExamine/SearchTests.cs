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

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public class SearchTests : ExamineBaseTest
    {

        [Test]
        public void Test_Sort_Order_Sorting()
        {
            //var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\SearchTests", Guid.NewGuid().ToString()));
            //System.IO.Directory.CreateDirectory(newIndexFolder.FullName);

            using (var luceneDir = new RAMDirectory())
            //using (var luceneDir = new SimpleFSDirectory(newIndexFolder))
            {
                var indexer = IndexInitializer.GetUmbracoIndexer(luceneDir, null,
                    new TestDataService()
                        {
                            ContentService = new TestContentService(TestFiles.umbraco_sort)
                        });
                indexer.RebuildIndex();
                var searcher = IndexInitializer.GetUmbracoSearcher(luceneDir);

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

        //[Test]
        //public void Test_Index_Type_With_German_Analyzer()
        //{
        //    using (var luceneDir = new RAMDirectory())
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
