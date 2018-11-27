using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using Moq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Examine;
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
            IProfilingLogger profilingLogger,
            Directory luceneDir,
            ISqlContext sqlContext,
            Analyzer analyzer = null,
            IContentService contentService = null,
            IMediaService mediaService = null,
            IMemberService memberService = null,
            IUserService userService = null,
            IContentTypeService contentTypeService = null,
            IMediaTypeService mediaTypeService = null,
            UmbracoContentIndexerOptions options = null)
        {
            if (contentService == null)
            {
                long longTotalRecs;
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
                            m.GetCultureName(It.IsAny<string>()) == (string)x.Attribute("nodeName") &&
                            m.Path == (string)x.Attribute("path") &&
                            m.Properties == new PropertyCollection() &&
                            m.ContentType == Mock.Of<IContentType>(mt =>
                                mt.Icon == "test" &&
                                mt.Alias == x.Name.LocalName &&
                                mt.Id == (int)x.Attribute("nodeType"))))
                    .ToArray();


                contentService = Mock.Of<IContentService>(
                    x => x.GetPagedDescendants(
                        It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out longTotalRecs, It.IsAny<IQuery<IContent>>(), It.IsAny<Ordering>())
                        ==
                        allRecs);
            }
            if (userService == null)
            {
                userService = Mock.Of<IUserService>(x => x.GetProfileById(It.IsAny<int>()) == Mock.Of<IProfile>(p => p.Id == 0 && p.Name == "admin"));
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
                            m.GetCultureName(It.IsAny<string>()) == (string)x.Attribute("nodeName") &&
                            m.Path == (string) x.Attribute("path") &&
                            m.Properties == new PropertyCollection() &&
                            m.ContentType == Mock.Of<IMediaType>(mt =>
                                mt.Alias == (string) x.Attribute("nodeTypeAlias") &&
                                mt.Id == (int) x.Attribute("nodeType"))))
                    .ToArray();

                // MOCK!
                var mediaServiceMock = new Mock<IMediaService>();

                mediaServiceMock
                    .Setup(x => x.GetPagedDescendants(
                            It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out totalRecs, It.IsAny<IQuery<IMedia>>(), It.IsAny<Ordering>())
                    ).Returns(() => allRecs);

                //mediaServiceMock.Setup(service => service.GetPagedXmlEntries(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), out longTotalRecs))
                //    .Returns(() => allRecs.Select(x => x.ToXml()));

                mediaService = mediaServiceMock.Object;

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

            if (mediaTypeService == null)
            {
                var mediaTypeServiceMock = new Mock<IMediaTypeService>();
                mediaTypeServiceMock.Setup(x => x.GetAll())
                    .Returns(new List<IMediaType>
                    {
                        new MediaType(-1) {Alias = "Folder", Name = "Folder", Id = 1031, Icon = "icon-folder"},
                        new MediaType(-1) {Alias = "Image", Name = "Image", Id = 1032, Icon = "icon-picture"}
                    });
                mediaTypeService = mediaTypeServiceMock.Object;
            }

            // fixme oops?!
            //var query = new Mock<IQuery<IContent>>();
            //query
            //    .Setup(x => x.GetWhereClauses())
            //    .Returns(new List<Tuple<string, object[]>> { new Tuple<string, object[]>($"{Constants.DatabaseSchema.Tables.Document}.published", new object[] { 1 }) });
            
            //scopeProvider
            //    .Setup(x => x.Query<IContent>())
            //    .Returns(query.Object);

            var i = new UmbracoContentIndexer(
                "testIndexer",
                Enumerable.Empty<FieldDefinition>(),
                luceneDir,
                analyzer,
                profilingLogger,
                contentService,
                mediaService,
                userService,
                sqlContext,
                new[] {new DefaultUrlSegmentProvider()},
                new UmbracoContentValueSetValidator(options, Mock.Of<IPublicAccessService>()),
                options);

            i.IndexingError += IndexingError;

            return i;
        }

        public static LuceneSearcher GetLuceneSearcher(Directory luceneDir)
        {
            return new LuceneSearcher("testSearcher", luceneDir, new StandardAnalyzer(Version.LUCENE_29));
        }

        public static MultiIndexSearcher GetMultiSearcher(Directory pdfDir, Directory simpleDir, Directory conventionDir, Directory cwsDir)
        {
            var i = new MultiIndexSearcher("testSearcher", new[] { pdfDir, simpleDir, conventionDir, cwsDir }, new StandardAnalyzer(Version.LUCENE_29));
            return i;
        }


        internal static void IndexingError(object sender, IndexingErrorEventArgs e)
        {
            throw new ApplicationException(e.Message, e.InnerException);
        }


    }
}
