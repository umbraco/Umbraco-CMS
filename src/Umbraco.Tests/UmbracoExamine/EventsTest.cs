using System;
using System.Linq;
using Examine;
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
			//change the parent id so that they are all ignored
			var existingCriteria = _indexer.IndexerData;
			_indexer.IndexerData = new IndexCriteria(existingCriteria.StandardFields, existingCriteria.UserFields, existingCriteria.IncludeNodeTypes, existingCriteria.ExcludeNodeTypes,
				999); //change to 999

			var isIgnored = false;

			EventHandler<IndexingNodeDataEventArgs> ignoringNode = (s, e) =>
				{
					isIgnored = true;
				};

			_indexer.IgnoringNode += ignoringNode;

			//get a node from the data repo
			var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
			                          .Root
			                          .Elements()
			                          .First();

			_indexer.ReIndexNode(node, IndexTypes.Content);


			Assert.IsTrue(isIgnored);

		}

		private readonly TestContentService _contentService = new TestContentService();
		private static UmbracoExamineSearcher _searcher;
		private static UmbracoContentIndexer _indexer;
		private Lucene.Net.Store.Directory _luceneDir;

		public override void TestSetup()
		{
            base.TestSetup();
			_luceneDir = new RAMDirectory();
			_indexer = IndexInitializer.GetUmbracoIndexer(_luceneDir);
			_indexer.RebuildIndex();
			_searcher = IndexInitializer.GetUmbracoSearcher(_luceneDir);
		}

		public override void TestTearDown()
		{
		    base.TestTearDown();
			_luceneDir.Dispose();
		}
	}
}