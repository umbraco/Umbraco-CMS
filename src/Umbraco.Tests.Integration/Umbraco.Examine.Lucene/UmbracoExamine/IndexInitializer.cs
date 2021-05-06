using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Persistence;
using IContentService = Umbraco.Cms.Core.Services.IContentService;
using IMediaService = Umbraco.Cms.Core.Services.IMediaService;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine
{
    /// <summary>
    /// Used internally by test classes to initialize a new index from the template
    /// </summary>
    public class IndexInitializer
    {
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IScopeProvider _scopeProvider;
        private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
        private readonly ILoggerFactory _loggerFactory;

        public IndexInitializer(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            PropertyEditorCollection propertyEditors,
            IScopeProvider scopeProvider,
            IUmbracoDatabaseFactory umbracoDatabaseFactory,
            ILoggerFactory loggerFactory)
        {
            _shortStringHelper = shortStringHelper;
            _jsonSerializer = jsonSerializer;
            _propertyEditors = propertyEditors;
            _scopeProvider = scopeProvider;
            _umbracoDatabaseFactory = umbracoDatabaseFactory;
            _loggerFactory = loggerFactory;
        }

        public ContentValueSetBuilder GetContentValueSetBuilder(bool publishedValuesOnly)
        {
            var contentValueSetBuilder = new ContentValueSetBuilder(
                _propertyEditors,
                new UrlSegmentProviderCollection(new[] { new DefaultUrlSegmentProvider(_shortStringHelper) }),
                GetMockUserService(),
                _shortStringHelper,
                _scopeProvider,
                publishedValuesOnly);

            return contentValueSetBuilder;
        }

        public ContentIndexPopulator GetContentIndexRebuilder(IContentService contentService, bool publishedValuesOnly)
        {
            var contentValueSetBuilder = GetContentValueSetBuilder(publishedValuesOnly);
            var contentIndexDataSource = new ContentIndexPopulator(publishedValuesOnly, null, contentService, _umbracoDatabaseFactory, contentValueSetBuilder);
            return contentIndexDataSource;
        }

        public MediaIndexPopulator GetMediaIndexRebuilder(IMediaService mediaService)
        {
            var mediaValueSetBuilder = new MediaValueSetBuilder(_propertyEditors, new UrlSegmentProviderCollection(new[] { new DefaultUrlSegmentProvider(_shortStringHelper) }), GetMockUserService(), Mock.Of<ILogger<MediaValueSetBuilder>>(), _shortStringHelper, _jsonSerializer);
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

        public IUserService GetMockUserService() => Mock.Of<IUserService>(x => x.GetProfileById(It.IsAny<int>()) == Mock.Of<IProfile>(p => p.Id == 0 && p.Name == "admin"));

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

        public ILocalizationService GetMockLocalizationService() => Mock.Of<ILocalizationService>(x => x.GetAllLanguages() == Array.Empty<ILanguage>());

        public static IMediaTypeService GetMockMediaTypeService(IShortStringHelper shortStringHelper)
        {
            var mediaTypeServiceMock = new Mock<IMediaTypeService>();
            mediaTypeServiceMock.Setup(x => x.GetAll())
                .Returns(new List<IMediaType>
                {
                    new MediaType(shortStringHelper, -1) {Alias = "Folder", Name = "Folder", Id = 1031, Icon = "icon-folder"},
                    new MediaType(shortStringHelper, -1) {Alias = "Image", Name = "Image", Id = 1032, Icon = "icon-picture"}
                });
            return mediaTypeServiceMock.Object;
        }

        public IProfilingLogger GetMockProfilingLogger() => new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>());

        public UmbracoContentIndex GetUmbracoIndexer(
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
                analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);

            if (validator == null)
                validator = new ContentValueSetValidator(true);

            var options = GetOptions(
                "testIndexer",
                new LuceneDirectoryIndexOptions
                {
                    Analyzer = analyzer,
                    Validator = validator,
                    DirectoryFactory = new GenericDirectoryFactory(s => luceneDir),
                    FieldDefinitions = new UmbracoFieldDefinitionCollection()
                });

            var i = new UmbracoContentIndex(
                _loggerFactory,
                "testIndexer",
                options,
                hostingEnvironment,
                runtimeState,
                languageService);

            i.IndexingError += IndexingError;
            i.IndexOperationComplete += I_IndexOperationComplete;

            return i;
        }

        private void I_IndexOperationComplete(object sender, IndexOperationEventArgs e)
        {
            
        }

        //public static MultiIndexSearcher GetMultiSearcher(Directory pdfDir, Directory simpleDir, Directory conventionDir, Directory cwsDir)
        //{
        //    var i = new MultiIndexSearcher("testSearcher", new[] { pdfDir, simpleDir, conventionDir, cwsDir }, new StandardAnalyzer(Version.LUCENE_29));
        //    return i;
        //}

        public static IOptionsSnapshot<LuceneDirectoryIndexOptions> GetOptions(string indexName, LuceneDirectoryIndexOptions options)
            => Mock.Of<IOptionsSnapshot<LuceneDirectoryIndexOptions>>(x => x.Get(indexName) == options);

        internal void IndexingError(object sender, IndexingErrorEventArgs e) => throw new ApplicationException(e.Message, e.Exception);


    }
}
