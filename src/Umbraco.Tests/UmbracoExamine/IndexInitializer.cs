﻿using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Moq;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Tests.TestHelpers;
using IContentService = Umbraco.Cms.Core.Services.IContentService;
using IMediaService = Umbraco.Cms.Core.Services.IMediaService;
using Version = Lucene.Net.Util.Version;

namespace Umbraco.Tests.UmbracoExamine
{
    /// <summary>
    /// Used internally by test classes to initialize a new index from the template
    /// </summary>
    internal static class IndexInitializer
    {
        public static ContentValueSetBuilder GetContentValueSetBuilder(PropertyEditorCollection propertyEditors, IScopeProvider scopeProvider, bool publishedValuesOnly)
        {
            var contentValueSetBuilder = new ContentValueSetBuilder(
                propertyEditors,
                new UrlSegmentProviderCollection(new[] { new DefaultUrlSegmentProvider(TestHelper.ShortStringHelper) }),
                GetMockUserService(),
                TestHelper.ShortStringHelper,
                scopeProvider,
                publishedValuesOnly);

            return contentValueSetBuilder;
        }

        public static ContentIndexPopulator GetContentIndexRebuilder(PropertyEditorCollection propertyEditors, IContentService contentService, IScopeProvider scopeProvider, IUmbracoDatabaseFactory umbracoDatabaseFactory, bool publishedValuesOnly)
        {
            var contentValueSetBuilder = GetContentValueSetBuilder(propertyEditors, scopeProvider, publishedValuesOnly);
            var contentIndexDataSource = new ContentIndexPopulator(publishedValuesOnly, null, contentService, umbracoDatabaseFactory, contentValueSetBuilder);
            return contentIndexDataSource;
        }

        public static MediaIndexPopulator GetMediaIndexRebuilder(PropertyEditorCollection propertyEditors, IMediaService mediaService)
        {
            var mediaValueSetBuilder = new MediaValueSetBuilder(propertyEditors, new UrlSegmentProviderCollection(new[] { new DefaultUrlSegmentProvider(TestHelper.ShortStringHelper) }), GetMockUserService(), Mock.Of<ILogger<MediaValueSetBuilder>>(), TestHelper.ShortStringHelper, TestHelper.JsonSerializer);
            var mediaIndexDataSource = new MediaIndexPopulator(null, mediaService, mediaValueSetBuilder);
            return mediaIndexDataSource;
        }
        public static IContentService GetMockContentService()
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
                        m.Name == (string)x.Attribute(UmbracoExamineFieldNames.NodeNameFieldName) &&
                        m.GetCultureName(It.IsAny<string>()) == (string)x.Attribute(UmbracoExamineFieldNames.NodeNameFieldName) &&
                        m.Path == (string)x.Attribute("path") &&
                        m.Properties == new PropertyCollection() &&
                        m.ContentType == Mock.Of<ISimpleContentType>(mt =>
                            mt.Icon == "test" &&
                            mt.Alias == x.Name.LocalName &&
                            mt.Id == (int)x.Attribute("nodeType"))))
                .ToArray();


            return Mock.Of<IContentService>(
                x => x.GetPagedDescendants(
                         It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out longTotalRecs, It.IsAny<IQuery<IContent>>(), It.IsAny<Ordering>())
                     == allRecs);
        }

        public static IUserService GetMockUserService()
        {
            return Mock.Of<IUserService>(x => x.GetProfileById(It.IsAny<int>()) == Mock.Of<IProfile>(p => p.Id == 0 && p.Name == "admin"));
        }

        public static IMediaService GetMockMediaService()
        {
            long totalRecs;

            var demoData = new ExamineDemoDataMediaService();

            var allRecs = demoData.GetLatestMediaByXpath("//node")
                .Root
                .Elements()
                .Select(x => Mock.Of<IMedia>(
                    m =>
                        m.Id == (int)x.Attribute("id") &&
                        m.ParentId == (int)x.Attribute("parentID") &&
                        m.Level == (int)x.Attribute("level") &&
                        m.CreatorId == 0 &&
                        m.SortOrder == (int)x.Attribute("sortOrder") &&
                        m.CreateDate == (DateTime)x.Attribute("createDate") &&
                        m.UpdateDate == (DateTime)x.Attribute("updateDate") &&
                        m.Name == (string)x.Attribute(UmbracoExamineFieldNames.NodeNameFieldName) &&
                        m.GetCultureName(It.IsAny<string>()) == (string)x.Attribute(UmbracoExamineFieldNames.NodeNameFieldName) &&
                        m.Path == (string)x.Attribute("path") &&
                        m.Properties == new PropertyCollection() &&
                        m.ContentType == Mock.Of<ISimpleContentType>(mt =>
                            mt.Alias == (string)x.Attribute("nodeTypeAlias") &&
                            mt.Id == (int)x.Attribute("nodeType"))))
                .ToArray();

            // MOCK!
            var mediaServiceMock = new Mock<IMediaService>();

            mediaServiceMock
                .Setup(x => x.GetPagedDescendants(
                        It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out totalRecs, It.IsAny<IQuery<IMedia>>(), It.IsAny<Ordering>())
                ).Returns(() => allRecs);

            //mediaServiceMock.Setup(service => service.GetPagedXmlEntries(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), out longTotalRecs))
            //    .Returns(() => allRecs.Select(x => x.ToXml()));

            return mediaServiceMock.Object;
        }

        public static ILocalizationService GetMockLocalizationService()
        {
            return Mock.Of<ILocalizationService>(x => x.GetAllLanguages() == Array.Empty<ILanguage>());
        }

        public static IMediaTypeService GetMockMediaTypeService()
        {
            var mediaTypeServiceMock = new Mock<IMediaTypeService>();
            mediaTypeServiceMock.Setup(x => x.GetAll())
                .Returns(new List<IMediaType>
                {
                    new MediaType(TestHelper.ShortStringHelper, -1) {Alias = "Folder", Name = "Folder", Id = 1031, Icon = "icon-folder"},
                    new MediaType(TestHelper.ShortStringHelper, -1) {Alias = "Image", Name = "Image", Id = 1032, Icon = "icon-picture"}
                });
            return mediaTypeServiceMock.Object;
        }

        public static IProfilingLogger GetMockProfilingLogger()
        {
            return new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>());
        }

        public static UmbracoContentIndex GetUmbracoIndexer(
            IProfilingLogger profilingLogger,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState,
            Directory luceneDir,
            Analyzer analyzer = null,
            ILocalizationService languageService = null,
            IContentValueSetValidator validator = null)
        {
            if (languageService == null)
                languageService = GetMockLocalizationService();

            if (analyzer == null)
                analyzer = new StandardAnalyzer(Version.LUCENE_30);

            if (validator == null)
                validator = new ContentValueSetValidator(true);

            var i = new UmbracoContentIndex(
                "testIndexer",
                luceneDir,
                new UmbracoFieldDefinitionCollection(),
                analyzer,
                profilingLogger,
                Mock.Of<ILogger<UmbracoContentIndex>>(),
                Mock.Of<ILoggerFactory>(),
                hostingEnvironment,
                runtimeState,
                languageService,
                validator);

            i.IndexingError += IndexingError;

            return i;
        }

        //public static MultiIndexSearcher GetMultiSearcher(Directory pdfDir, Directory simpleDir, Directory conventionDir, Directory cwsDir)
        //{
        //    var i = new MultiIndexSearcher("testSearcher", new[] { pdfDir, simpleDir, conventionDir, cwsDir }, new StandardAnalyzer(Version.LUCENE_29));
        //    return i;
        //}


        internal static void IndexingError(object sender, IndexingErrorEventArgs e)
        {
            throw new ApplicationException(e.Message, e.InnerException);
        }


    }
}
