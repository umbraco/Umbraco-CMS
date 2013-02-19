using System;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using UmbracoExamine;
using UmbracoExamine.Config;
using UmbracoExamine.DataServices;
using UmbracoExamine.PDF;

namespace Umbraco.Tests.UmbracoExamine
{
	/// <summary>
	/// Used internally by test classes to initialize a new index from the template
	/// </summary>
	internal static class IndexInitializer
	{
		public static UmbracoContentIndexer GetUmbracoIndexer(
            Lucene.Net.Store.Directory luceneDir, 
            Analyzer analyzer = null,
            IDataService dataService = null)
		{
            if (dataService == null)
            {
                dataService = new TestDataService();
            }

            if (analyzer == null)
            {
                analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            }

		    var indexSet = new IndexSet();
		    var indexCriteria = indexSet.ToIndexCriteria(dataService);

		    var i = new UmbracoContentIndexer(indexCriteria,
		                                      luceneDir, //custom lucene directory
                                              dataService,
		                                      analyzer,
		                                      false);

			//i.IndexSecondsInterval = 1;

			i.IndexingError += IndexingError;

			return i;
		}
        public static UmbracoExamineSearcher GetUmbracoSearcher(Lucene.Net.Store.Directory luceneDir, Analyzer analyzer = null)
		{
            if (analyzer == null)
            {
                analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            }
            return new UmbracoExamineSearcher(luceneDir, analyzer);
		}
		//public static SimpleDataIndexer GetSimpleIndexer(Lucene.Net.Store.Directory luceneDir)
		//{
		//	var i = new SimpleDataIndexer(new IndexCriteria(
		//									  new IIndexField[] { },
		//									  new[]
		//										  {
		//											  new TestIndexField { Name = "Author" }, 
		//											  new TestIndexField { Name = "DateCreated", EnableSorting = true, Type = "DateTime"  },
		//											  new TestIndexField { Name = "Title" }, 
		//											  new TestIndexField { Name = "Photographer" }, 
		//											  new TestIndexField { Name = "YearCreated", Type = "Date.Year" }, 
		//											  new TestIndexField { Name = "MonthCreated", Type = "Date.Month" }, 
		//											  new TestIndexField { Name = "DayCreated", Type = "Date.Day" },
		//											  new TestIndexField { Name = "HourCreated", Type = "Date.Hour" },
		//											  new TestIndexField { Name = "MinuteCreated", Type = "Date.Minute" },
		//											  new TestIndexField { Name = "SomeNumber", Type = "Number" },
		//											  new TestIndexField { Name = "SomeFloat", Type = "Float" },
		//											  new TestIndexField { Name = "SomeDouble", Type = "Double" },
		//											  new TestIndexField { Name = "SomeLong", Type = "Long" }
		//										  },
		//									  new string[] { },
		//									  new string[] { },
		//									  -1),
		//								  luceneDir,
		//								  new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29),
		//								  new TestSimpleDataProvider(),
		//								  new[] { "Documents", "Pictures" },
		//								  false);
		//	i.IndexingError += IndexingError;

		//	return i;
		//}
		public static LuceneSearcher GetLuceneSearcher(Lucene.Net.Store.Directory luceneDir)
		{
			return new LuceneSearcher(luceneDir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
		}
		public static PDFIndexer GetPdfIndexer(Lucene.Net.Store.Directory luceneDir)
		{
			var i = new PDFIndexer(luceneDir,
									  new TestDataService(),
									  new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29),
									  false);

			i.IndexingError += IndexingError;

			return i;
		}
		public static MultiIndexSearcher GetMultiSearcher(Lucene.Net.Store.Directory pdfDir, Lucene.Net.Store.Directory simpleDir, Lucene.Net.Store.Directory conventionDir, Lucene.Net.Store.Directory cwsDir)
		{
			var i = new MultiIndexSearcher(new[] { pdfDir, simpleDir, conventionDir, cwsDir }, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
			return i;
		}


		internal static void IndexingError(object sender, IndexingErrorEventArgs e)
		{
			throw new ApplicationException(e.Message, e.InnerException);
		}


	}
}