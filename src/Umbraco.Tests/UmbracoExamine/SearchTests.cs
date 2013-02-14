using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Store;
using NUnit.Framework;
using Umbraco.Tests.PartialTrust;
using UmbracoExamine;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public class SearchTests : AbstractPartialTrustFixture<IndexTest>
    {

        [Test]
        public void Test_Index_Type_With_German_Analyzer()
        {
            using (var luceneDir = new RAMDirectory())
            {
                var indexer = IndexInitializer.GetUmbracoIndexer(luceneDir, null, new TestDataService()
                    {
                        ContentService = new TestContentService(TestFiles.umbraco_sort)
                    });
                indexer.RebuildIndex();
                var searcher = IndexInitializer.GetUmbracoSearcher(luceneDir);

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
