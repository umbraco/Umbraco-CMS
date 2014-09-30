using System;
using System.Linq;
using System.Net.Mime;
using Examine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Moq;
using Umbraco.Core.Services;
using UmbracoExamine;
using UmbracoExamine.Config;
using UmbracoExamine.DataServices;
using UmbracoExamine.PDF;
using IContentService = UmbracoExamine.DataServices.IContentService;
using IMediaService = UmbracoExamine.DataServices.IMediaService;

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
            IDataService dataService = null,
            Umbraco.Core.Services.IContentService contentService = null,
            Umbraco.Core.Services.IMediaService mediaService = null,
            IDataTypeService dataTypeService = null,
            IContentTypeService contentTypeService = null,
            IMemberService memberService = null)
		{
            if (dataService == null)
            {
                dataService = new TestDataService();
            }
		    if (contentService == null)
		    {
                contentService = Mock.Of<Umbraco.Core.Services.IContentService>();
		    }
            if (mediaService == null)
            {
                mediaService = Mock.Of<Umbraco.Core.Services.IMediaService>();
            }
            if (dataTypeService == null)
            {
                dataTypeService = Mock.Of<IDataTypeService>();
            }
            if (contentTypeService == null)
            {
                contentTypeService = Mock.Of<IContentTypeService>();
            }
            if (memberService == null)
            {
                memberService = Mock.Of<IMemberService>();
            }

            if (analyzer == null)
            {
                analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            }

		    var indexSet = new IndexSet();
            var indexCriteria = indexSet.ToIndexCriteria(dataService, UmbracoContentIndexer.IndexFieldPolicies);

		    var i = new UmbracoContentIndexer(indexCriteria,
		                                      luceneDir, //custom lucene directory
                                              dataService,
                                              contentService,
                                              mediaService,
                                              dataTypeService,
                                              contentTypeService,
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