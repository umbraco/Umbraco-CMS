using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Store;
using NUnit.Framework;
using Umbraco.Tests.PartialTrust;
using UmbracoExamine;
using Examine.LuceneEngine.SearchCriteria;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public class SearchTests : AbstractPartialTrustFixture<SearchTests>
    {

        [Test]
        public void Test_Sort_Order_Sorting()
        {
            using (var luceneDir = new RAMDirectory())
            {
                //var indexer = IndexInitializer.GetUmbracoIndexer(luceneDir, null, new TestDataService()
                //    {
                //        ContentService = new TestContentService(TestFiles.umbraco_sort)
                //    });
                var indexer = IndexInitializer.GetUmbracoIndexer(luceneDir);
                indexer.RebuildIndex();
                var searcher = IndexInitializer.GetUmbracoSearcher(luceneDir);

                var s = (LuceneSearcher) searcher;
                var luceneSearcher = s.GetSearcher();
                var i = (LuceneIndexer) indexer;

                var criteria = searcher.CreateSearchCriteria();
                //var filter = criteria.ParentId(1148).And().OrderBy(new SortableField("sortOrder", SortType.Int));
                var filter = criteria.ParentId(1148);
                var result = searcher.Search(filter.Compile());
                Assert.AreEqual(12, result.TotalItemCount);

            }
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

        public override void TestTearDown()
        {
            
        }

        public override void TestSetup()
        {
            
        }
    }
}
