using System;
using System.Linq;
using Examine;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using UmbracoExamine;

namespace Umbraco.Tests.UmbracoExamine
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
	public class EventsTest : ExamineBaseTest
	{
		[Test]
		public void Events_Ignoring_Node()
		{
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var writer = new IndexWriter(luceneDir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), IndexWriter.MaxFieldLength.LIMITED))
            using (var indexer = IndexInitializer.GetUmbracoIndexer(writer))
		    using (var searcher = IndexInitializer.GetUmbracoSearcher(writer))
		    {
                //change the parent id so that they are all ignored
                var existingCriteria = indexer.IndexerData;
                indexer.IndexerData = new IndexCriteria(existingCriteria.StandardFields, existingCriteria.UserFields, existingCriteria.IncludeNodeTypes, existingCriteria.ExcludeNodeTypes,
                    999); //change to 999

                var isIgnored = false;

                EventHandler<IndexingNodeDataEventArgs> ignoringNode = (s, e) =>
                {
                    isIgnored = true;
                };

                indexer.IgnoringNode += ignoringNode;

                //get a node from the data repo
                var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                                          .Root
                                          .Elements()
                                          .First();

                indexer.ReIndexNode(node, IndexTypes.Content);


                Assert.IsTrue(isIgnored);
            }
		}

		private readonly TestContentService _contentService = new TestContentService();
        
	}
}