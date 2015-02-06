using System;
using System.Linq;
using System.Net.Mime;
using System.Xml.XPath;
using Examine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Moq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
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
            IMemberService memberService = null,
            IUserService userService = null)
		{
            if (dataService == null)
            {
                dataService = new TestDataService();
            }
		    if (contentService == null)
		    {
                contentService = Mock.Of<Umbraco.Core.Services.IContentService>();
		    }
		    if (userService == null)
		    {
		        userService = Mock.Of<IUserService>(x => x.GetProfileById(It.IsAny<int>()) == Mock.Of<IProfile>(p => p.Id == (object)0 && p.Name == "admin"));
		    }
            if (mediaService == null)
            {
                int totalRecs;

                var allRecs = dataService.MediaService.GetLatestMediaByXpath("//node")
                    .Root
                    .Elements()
                    .Select(x => Mock.Of<IMedia>(
                        m =>
                            m.Id == (int) x.Attribute("id") &&
                            m.ParentId == (int) x.Attribute("parentID") &&
                            m.Level == (int) x.Attribute("level") &&
                            m.CreatorId == 0 &&
                            m.SortOrder == (int) x.Attribute("sortOrder") &&
                            m.CreateDate == (DateTime) x.Attribute("createDate") &&
                            m.UpdateDate == (DateTime) x.Attribute("updateDate") &&
                            m.Name == (string) x.Attribute("nodeName") &&
                            m.Path == (string) x.Attribute("path") &&
                            m.Properties == new PropertyCollection() &&
                            m.ContentType == Mock.Of<IMediaType>(mt =>
                                mt.Alias == (string) x.Attribute("nodeTypeAlias") &&
                                mt.Id == (int) x.Attribute("nodeType"))))
                    .ToArray();
                    

                mediaService = Mock.Of<Umbraco.Core.Services.IMediaService>(
                    x => x.GetPagedDescendants(
                        It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), out totalRecs, It.IsAny<string>(), It.IsAny<Direction>(), It.IsAny<string>()) 
                        ==
                        allRecs);
            }
            if (dataTypeService == null)
            {
                dataTypeService = Mock.Of<IDataTypeService>();
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
                                              userService,
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