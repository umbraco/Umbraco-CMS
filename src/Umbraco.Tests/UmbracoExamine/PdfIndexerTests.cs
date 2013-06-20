using System.Diagnostics;
using System.IO;
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
    public class PdfIndexerTests  // itextsharp is not med trust safe so can't use hte base class: ExamineBaseTest<PdfIndexerTests>
	{

		private readonly TestMediaService _mediaService = new TestMediaService();
		private static PDFIndexer _indexer;
		private static UmbracoExamineSearcher _searcher;
		private Lucene.Net.Store.Directory _luceneDir;
		
        [SetUp]
		public void TestSetup()
		{
            UmbracoExamineSearcher.DisableInitializationCheck = true;
            BaseUmbracoIndexer.DisableInitializationCheck = true;
            //we'll copy over the pdf files first
		    var svc = new TestDataService();
		    var path = svc.MapPath("/App_Data/Converting_file_to_PDF.pdf");
		    var f = new FileInfo(path);
		    var dir = f.Directory;
            //ensure the folder is there
            System.IO.Directory.CreateDirectory(dir.FullName);
            var pdfs = new[] { TestFiles.Converting_file_to_PDF, TestFiles.PDFStandards, TestFiles.SurviorFlipCup, TestFiles.windows_vista };
            var names = new[] { "Converting_file_to_PDF.pdf", "PDFStandards.pdf", "SurviorFlipCup.pdf", "windows_vista.pdf" };
		    for (int index = 0; index < pdfs.Length; index++)
		    {
		        var p = pdfs[index];
		        using (var writer = File.Create(Path.Combine(dir.FullName, names[index])))
		        {
		            writer.Write(p, 0, p.Length);
		        }
		    }

		    _luceneDir = new RAMDirectory();
			_indexer = IndexInitializer.GetPdfIndexer(_luceneDir);
			_indexer.RebuildIndex();
			_searcher = IndexInitializer.GetUmbracoSearcher(_luceneDir);
		}

		[TearDown]
		public void TestTearDown()
		{
            UmbracoExamineSearcher.DisableInitializationCheck = null;
            BaseUmbracoIndexer.DisableInitializationCheck = null;
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
			
			//search the pdf content to ensure it's there
		    var contents = _searcher.Search(_searcher.CreateSearchCriteria().Id(1113).Compile()).Single()
		                            .Fields[PDFIndexer.TextContentFieldName]; 
			Assert.IsTrue(contents.Contains("Fonts are automatically embedded in Word 2008"));

		    contents = _searcher.Search(_searcher.CreateSearchCriteria().Id(1114).Compile()).Single()
		                        .Fields[PDFIndexer.TextContentFieldName];
            Assert.IsTrue(contents.Contains("Drink the beer and then flip the cup"));

            //NOTE: This is one of those PDFs that cannot be read and not sure how to force it too.
            // Will leave this here as one day we might figure it out.
            //contents = _searcher.Search(_searcher.CreateSearchCriteria().Id(1115).Compile()).Single()
            //                       .Fields[PDFIndexer.TextContentFieldName];
            //Assert.IsTrue(contents.Contains("Activation associates the use of the software"));

		    contents = _searcher.Search(_searcher.CreateSearchCriteria().Id(1116).Compile()).Single()
		                        .Fields[PDFIndexer.TextContentFieldName];
            Assert.IsTrue(contents.Contains("This lack of standardization could be chaotic"));


		}
	}
}