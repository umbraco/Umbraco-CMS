using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using Moq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using UmbracoExamine;
using IContentService = Umbraco.Core.Services.IContentService;
using IMediaService = Umbraco.Core.Services.IMediaService;
using Version = Lucene.Net.Util.Version;

namespace Umbraco.Tests.UmbracoExamine
{
	/// <summary>
	/// Used internally by test classes to initialize a new index from the template
	/// </summary>
	internal static class IndexInitializer
	{
		public static UmbracoContentIndexer GetUmbracoIndexer(
            ProfilingLogger profilingLogger,
            Directory luceneDir, 
            Analyzer analyzer = null,
            IContentService contentService = null,
            IMediaService mediaService = null,
            IMemberService memberService = null,
            IUserService userService = null,
            UmbracoContentIndexerOptions options = null)
		{            
		    if (contentService == null)
		    {
                long totalRecs;

                var demoData = new ExamineDemoDataContentService();

                var allRecs = demoData.GetLatestContentByXPath("//*[@isDoc]")
                    .Root
                    .Elements()
                    .Select(x => Mock.Of<IContent>(
                        m =>
                            m.Id == (int)x.Attribute("id") &&
                            m.ParentId == (int)x.Attribute("parentID") &&
                            m.Level == (int)x.Attribute("level") &&
                            m.CreatorId == 0 &&
                            m.SortOrder == (int)x.Attribute("sortOrder") &&
                            m.CreateDate == (DateTime)x.Attribute("createDate") &&
                            m.UpdateDate == (DateTime)x.Attribute("updateDate") &&
                            m.Name == (string)x.Attribute("nodeName") &&
                            m.Path == (string)x.Attribute("path") &&
                            m.Properties == new PropertyCollection() &&
                            m.ContentType == Mock.Of<IContentType>(mt =>
                                mt.Icon == "test" &&
                                mt.Alias == x.Name.LocalName &&
                                mt.Id == (int)x.Attribute("nodeType"))))
                    .ToArray();


                contentService = Mock.Of<IContentService>(
                    x => x.GetPagedDescendants(
                        It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out totalRecs, It.IsAny<string>(), It.IsAny<Direction>(), It.IsAny<string>())
                        ==
                        allRecs
                        
                        &&

                        x.GetPagedDescendants(
                        It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out totalRecs, It.IsAny<string>(), It.IsAny<Direction>(), It.IsAny<bool>(), It.IsAny<IQuery<IContent>>())
                        ==
                        allRecs); 
            }
		    if (userService == null)
		    {
		        userService = Mock.Of<IUserService>(x => x.GetProfileById(It.IsAny<int>()) == Mock.Of<IProfile>(p => p.Id == (object)0 && p.Name == "admin"));
		    }
            if (mediaService == null)
            {
                long totalRecs;

                var demoData = new ExamineDemoDataMediaService();

                var allRecs = demoData.GetLatestMediaByXpath("//node")
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
                    

                mediaService = Mock.Of<IMediaService>(
                    x => x.GetPagedDescendants(
                        It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out totalRecs, It.IsAny<string>(), It.IsAny<Direction>(), It.IsAny<string>()) 
                        ==
                        allRecs

                        &&

                        x.GetPagedDescendants(
                        It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out totalRecs, It.IsAny<string>(), It.IsAny<Direction>(), It.IsAny<bool>(), It.IsAny<IQuery<IMedia>>())
                        ==
                        allRecs);
            }
            
            if (analyzer == null)
            {
                analyzer = new StandardAnalyzer(Version.LUCENE_30);
            }

		    //var indexSet = new IndexSet();
      //      var indexCriteria = indexSet.ToIndexCriteria(dataService, UmbracoContentIndexer.IndexFieldPolicies);
            
		    //var i = new UmbracoContentIndexer(indexCriteria,
		    //                                  luceneDir, //custom lucene directory
      //                                        dataService,
      //                                        contentService,
      //                                        mediaService,
      //                                        dataTypeService,
      //                                        userService,
      //                                        new[] { new DefaultUrlSegmentProvider() },
		    //                                  analyzer,
		    //                                  false);

			//i.IndexSecondsInterval = 1;

		    if (options == null)
		    {
                options = new UmbracoContentIndexerOptions(false, false, null);
            }

		    var i = new UmbracoContentIndexer(
		        new[]
		        {
		            new FieldDefinition("", FieldDefinitionTypes.FullText)
		        },
		        luceneDir,
		        analyzer,
		        profilingLogger,
		        contentService,
		        mediaService,
		        userService,
		        new[] {new DefaultUrlSegmentProvider()},
		        new UmbracoContentValueSetValidator(options, Mock.Of<IPublicAccessService>()),
		        options,
                //TODO: This should be rewritten without the Mock fluent syntax so it can be read better, 
                // but I'll do that some other time, for this now just mocks the GetWhereClauses to return the 
                // correct clauses that are expected
		        Mock.Of<IQueryFactory>(
		            factory => factory.Create<IContent>() == Mock.Of<IQuery<IContent>>(
		                query => query.GetWhereClauses() == new List<Tuple<string, object[]>> {new Tuple<string, object[]>("cmsDocument.published", new object[] {1})})));

			i.IndexingError += IndexingError;

			return i;
		}
        
		public static LuceneSearcher GetLuceneSearcher(Directory luceneDir)
		{
			return new LuceneSearcher(luceneDir, new StandardAnalyzer(Version.LUCENE_29));
		}
		
		public static MultiIndexSearcher GetMultiSearcher(Directory pdfDir, Directory simpleDir, Directory conventionDir, Directory cwsDir)
		{
			var i = new MultiIndexSearcher(new[] { pdfDir, simpleDir, conventionDir, cwsDir }, new StandardAnalyzer(Version.LUCENE_29));
			return i;
		}


		internal static void IndexingError(object sender, IndexingErrorEventArgs e)
		{
			throw new ApplicationException(e.Message, e.Exception);
		}


	}
}