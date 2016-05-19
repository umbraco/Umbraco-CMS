using System;
using System.Linq;
using Examine;
using Examine.Session;
using Lucene.Net.Store;
using NUnit.Framework;
using UmbracoExamine;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public class EventsTest : ExamineBaseTest
    {
        [Test]
        public void Events_Ignoring_Node()
        {
            using (var luceneDir = new RAMDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                //make parent id 999 so all are ignored
                options: new UmbracoContentIndexerOptions(false, false, 999)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))
            {
                var searcher = indexer.GetSearcher();
                
                var isIgnored = false;

                EventHandler<IndexingNodeDataEventArgs> ignoringNode = (s, e) =>
                {
                    isIgnored = true;
                };

                indexer.IgnoringNode += ignoringNode;

                var contentService = new ExamineDemoDataContentService();
                //get a node from the data repo
                var node = contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                                          .Root
                                          .Elements()
                                          .First();

                indexer.ReIndexNode(node, IndexTypes.Content);


                Assert.IsTrue(isIgnored);
            }



        }

    }
}