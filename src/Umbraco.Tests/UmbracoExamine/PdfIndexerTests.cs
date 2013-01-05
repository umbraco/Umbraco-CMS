using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Examine;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using UmbracoExamine;
using UmbracoExamine.PDF;

namespace Umbraco.Tests.UmbracoExamine
{
	[TestFixture]
	public class PdfIndexerTests
	{

		private readonly TestMediaService _mediaService = new TestMediaService();
		private static PDFIndexer _indexer;
		private static UmbracoExamineSearcher _searcher;
		private Lucene.Net.Store.Directory _luceneDir;

		[SetUp]
		public void Initialize()
		{
			_luceneDir = new RAMDirectory();
			_indexer = IndexInitializer.GetPdfIndexer(_luceneDir);
			_indexer.RebuildIndex();
			_searcher = IndexInitializer.GetUmbracoSearcher(_luceneDir);
		}

		[TearDown]
		public void TearDown()
		{
			_luceneDir.Dispose();
		}

		[Test]
		public void PDFIndexer_Ensure_ParentID_Honored()
		{
			//change parent id to 1116
			var existingCriteria = ((IndexCriteria)_indexer.IndexerData);
			_indexer.IndexerData = new IndexCriteria(existingCriteria.StandardFields, existingCriteria.UserFields, existingCriteria.IncludeNodeTypes, existingCriteria.ExcludeNodeTypes,
			                                         1116);

			//get the 2112 pdf node: 2112
			var node = _mediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
			                        .Root
			                        .Elements()
			                        .Where(x => (int)x.Attribute("id") == 2112)
			                        .First();

			//create a copy of 2112 undneath 1111 which is 'not indexable'
			var newpdf = XElement.Parse(node.ToString());
			newpdf.SetAttributeValue("id", "999999");
			newpdf.SetAttributeValue("path", "-1,1111,999999");
			newpdf.SetAttributeValue("parentID", "1111");

			//now reindex
			_indexer.ReIndexNode(newpdf, IndexTypes.Media);

			//make sure it doesn't exist

			var results = _searcher.Search(_searcher.CreateSearchCriteria().Id(999999).Compile());
			Assert.AreEqual(0, results.Count());
		}

		[Test]
		public void PDFIndexer_Reindex()
		{
			//get searcher and reader to get stats            
			var r = ((IndexSearcher)_searcher.GetSearcher()).GetIndexReader();

			Trace.Write("Num docs = " + r.NumDocs().ToString());

			Assert.AreEqual(7, r.NumDocs());

			//search the pdf content to ensure it's there
			Assert.IsTrue(_searcher.Search(_searcher.CreateSearchCriteria().Id(1113).Compile()).Single()
			                       .Fields[PDFIndexer.TextContentFieldName].Contains("EncapsulateField"));
			Assert.IsTrue(_searcher.Search(_searcher.CreateSearchCriteria().Id(1114).Compile()).Single()
			                       .Fields[PDFIndexer.TextContentFieldName].Contains("metaphysical realism"));

			//the contour PDF cannot be read properly, this is to due with the PDF encoding!
			//Assert.IsTrue(s.Search(s.CreateSearchCriteria().Id(1115).Compile()).Single()
			//    .Fields[PDFIndexer.TextContentFieldName].Contains("Returns All records from the form with the id"));

			Assert.IsTrue(_searcher.Search(_searcher.CreateSearchCriteria().Id(1116).Compile()).Single()
			                       .Fields[PDFIndexer.TextContentFieldName].Contains("What long-term preservation"));


		}
	}
}