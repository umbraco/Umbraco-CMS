using System.Linq;
using System.Threading;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using Umbraco.Tests.Testing;
using Umbraco.Examine;

namespace Umbraco.Tests.UmbracoExamine
{
    /// <summary>
    /// Tests the standard indexing capabilities
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class IndexTest : ExamineBaseTest
    {

        [Test]
        public void Rebuild_Index()
        {
            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir, ScopeProvider.SqlContext, options: new UmbracoContentIndexerOptions(true, false, null)))
            using (indexer.ProcessNonAsync())
            {
                var searcher = indexer.GetSearcher();

                //create the whole thing
                indexer.RebuildIndex();

                var result = searcher.Search(searcher.CreateCriteria().All().Compile());

                Assert.AreEqual(29, result.TotalItemCount);
            }
        }

        ///// <summary>
        /// <summary>
        /// Check that the node signalled as protected in the content service is not present in the index.
        /// </summary>
        [Test]
        public void Index_Protected_Content_Not_Indexed()
        {
            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir, ScopeProvider.SqlContext))
            using (indexer.ProcessNonAsync())
            using (var searcher = ((LuceneSearcher)indexer.GetSearcher()).GetLuceneSearcher())
            {
                //create the whole thing
                indexer.RebuildIndex();

                var protectedQuery = new BooleanQuery();
                protectedQuery.Add(
                    new BooleanClause(
                        new TermQuery(new Term(LuceneIndexer.CategoryFieldName, IndexTypes.Content)),
                        Occur.MUST));

                protectedQuery.Add(
                    new BooleanClause(
                        new TermQuery(new Term(LuceneIndexer.ItemIdFieldName, ExamineDemoDataContentService.ProtectedNode.ToString())),
                        Occur.MUST));

                var collector = TopScoreDocCollector.Create(100, true);

                searcher.Search(protectedQuery, collector);

                Assert.AreEqual(0, collector.TotalHits, "Protected node should not be indexed");
            }

        }

        [Test]
        public void Index_Move_Media_From_Non_Indexable_To_Indexable_ParentID()
        {
            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir, ScopeProvider.SqlContext,
                //make parent id 1116
                options: new UmbracoContentIndexerOptions(false, false, 1116)))
            using (indexer.ProcessNonAsync())
            {
                var searcher = indexer.GetSearcher();

                //get a node from the data repo (this one exists underneath 2222)
                var node = _mediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
                                        .Root
                                        .Elements()
                                        .Where(x => (int)x.Attribute("id") == 2112)
                                        .First();

                var currPath = (string)node.Attribute("path"); //should be : -1,1111,2222,2112
                Assert.AreEqual("-1,1111,2222,2112", currPath);

                //ensure it's indexed
                indexer.IndexItems(new []{ node.ConvertToValueSet(IndexTypes.Media) });

                //it will not exist because it exists under 2222
                var results = searcher.Search(searcher.CreateCriteria().Id(2112).Compile());
                Assert.AreEqual(0, results.Count());

                //now mimic moving 2112 to 1116
                //node.SetAttributeValue("path", currPath.Replace("2222", "1116"));
                node.SetAttributeValue("path", "-1,1116,2112");
                node.SetAttributeValue("parentID", "1116");

                //now reindex the node, this should first delete it and then WILL add it because of the parent id constraint
                indexer.IndexItems(new[] { node.ConvertToValueSet(IndexTypes.Media) });

                //now ensure it exists
                results = searcher.Search(searcher.CreateCriteria().Id(2112).Compile());
                Assert.AreEqual(1, results.Count());
            }
        }

        [Test]
        public void Index_Move_Media_To_Non_Indexable_ParentID()
        {
            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer1 = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir, ScopeProvider.SqlContext,
                //make parent id 2222
                options: new UmbracoContentIndexerOptions(false, false, 2222)))
            using (indexer1.ProcessNonAsync())
            {
                var searcher = indexer1.GetSearcher();

                //get a node from the data repo (this one exists underneath 2222)
                var node = _mediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
                                    .Root
                                    .Elements()
                                    .Where(x => (int)x.Attribute("id") == 2112)
                                    .First();

                var currPath = (string)node.Attribute("path"); //should be : -1,1111,2222,2112
                Assert.AreEqual("-1,1111,2222,2112", currPath);

                //ensure it's indexed
                indexer1.IndexItems(new[] { node.ConvertToValueSet(IndexTypes.Media) });



                //it will exist because it exists under 2222
                var results = searcher.Search(searcher.CreateCriteria().Id(2112).Compile());
                Assert.AreEqual(1, results.Count());

                //now mimic moving the node underneath 1116 instead of 2222
                node.SetAttributeValue("path", currPath.Replace("2222", "1116"));
                node.SetAttributeValue("parentID", "1116");

                //now reindex the node, this should first delete it and then NOT add it because of the parent id constraint
                indexer1.IndexItems(new[] { node.ConvertToValueSet(IndexTypes.Media) });



                //now ensure it's deleted
                results = searcher.Search(searcher.CreateCriteria().Id(2112).Compile());
                Assert.AreEqual(0, results.Count());
            }
        }


        /// <summary>
        /// This will ensure that all 'Content' (not media) is cleared from the index using the Lucene API directly.
        /// We then call the Examine method to re-index Content and do some comparisons to ensure that it worked correctly.
        /// </summary>
        [Test]
        public void Index_Reindex_Content()
        {
            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir, ScopeProvider.SqlContext, options: new UmbracoContentIndexerOptions(true, false, null)))
            using (indexer.ProcessNonAsync())
            {
                var searcher = indexer.GetSearcher();

                //create the whole thing
                indexer.RebuildIndex();


                var result = searcher.Search(searcher.CreateCriteria().Field(LuceneIndexer.CategoryFieldName, IndexTypes.Content).Compile());
                Assert.AreEqual(21, result.TotalItemCount);

                //delete all content
                foreach (var r in result)
                {
                    indexer.DeleteFromIndex(r.Id);
                }


                //ensure it's all gone
                result = searcher.Search(searcher.CreateCriteria().Field(LuceneIndexer.CategoryFieldName, IndexTypes.Content).Compile());
                Assert.AreEqual(0, result.TotalItemCount);

                //call our indexing methods
                indexer.IndexAll(IndexTypes.Content);



                result = searcher.Search(searcher.CreateCriteria().Field(LuceneIndexer.CategoryFieldName, IndexTypes.Content).Compile());
                Assert.AreEqual(21, result.TotalItemCount);
            }
        }

        /// <summary>
        /// This will delete an item from the index and ensure that all children of the node are deleted too!
        /// </summary>
        [Test]
        public void Index_Delete_Index_Item_Ensure_Heirarchy_Removed()
        {
            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir, ScopeProvider.SqlContext))
            using (indexer.ProcessNonAsync())
            {
                var searcher = indexer.GetSearcher();

                //create the whole thing
                indexer.RebuildIndex();


                //now delete a node that has children

                indexer.DeleteFromIndex(1140.ToString());
                //this node had children: 1141 & 1142, let's ensure they are also removed

                var results = searcher.Search(searcher.CreateCriteria().Id(1141).Compile());
                Assert.AreEqual(0, results.Count());

                results = searcher.Search(searcher.CreateCriteria().Id(1142).Compile());
                Assert.AreEqual(0, results.Count());

            }
        }

        private readonly ExamineDemoDataMediaService _mediaService = new ExamineDemoDataMediaService();
    }
}
