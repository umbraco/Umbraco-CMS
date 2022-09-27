using Examine;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Cms.Core.Configuration.Models;
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
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

/// <summary>
///     Used internally by test classes to initialize a new index from the template
/// </summary>
public class IndexInitializer
{
    private readonly IOptions<ContentSettings> _contentSettings;
    private readonly ILoggerFactory _loggerFactory;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IScopeProvider _scopeProvider;
    private readonly IShortStringHelper _shortStringHelper;

    public IndexInitializer(
        IShortStringHelper shortStringHelper,
        PropertyEditorCollection propertyEditors,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IScopeProvider scopeProvider,
        ILoggerFactory loggerFactory,
        IOptions<ContentSettings> contentSettings)
    {
        _shortStringHelper = shortStringHelper;
        _propertyEditors = propertyEditors;
        _mediaUrlGenerators = mediaUrlGenerators;
        _scopeProvider = scopeProvider;
        _loggerFactory = loggerFactory;
        _contentSettings = contentSettings;
    }

    public ContentValueSetBuilder GetContentValueSetBuilder(bool publishedValuesOnly)
    {
        var contentValueSetBuilder = new ContentValueSetBuilder(
            _propertyEditors,
            new UrlSegmentProviderCollection(() => new[] { new DefaultUrlSegmentProvider(_shortStringHelper) }),
            GetMockUserService(),
            _shortStringHelper,
            _scopeProvider,
            publishedValuesOnly);

        return contentValueSetBuilder;
    }

    public ContentIndexPopulator GetContentIndexRebuilder(IContentService contentService, bool publishedValuesOnly, IUmbracoDatabaseFactory umbracoDatabaseFactory)
    {
        var contentValueSetBuilder = GetContentValueSetBuilder(publishedValuesOnly);
        var contentIndexDataSource = new ContentIndexPopulator(
            _loggerFactory.CreateLogger<ContentIndexPopulator>(),
            publishedValuesOnly,
            null,
            contentService,
            umbracoDatabaseFactory,
            contentValueSetBuilder);
        return contentIndexDataSource;
    }

    public MediaIndexPopulator GetMediaIndexRebuilder(IMediaService mediaService)
    {
        var mediaValueSetBuilder = new MediaValueSetBuilder(
            _propertyEditors,
            new UrlSegmentProviderCollection(() => new[] { new DefaultUrlSegmentProvider(_shortStringHelper) }),
            _mediaUrlGenerators,
            GetMockUserService(),
            _shortStringHelper,
            _contentSettings);
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
            .Select((xmlElement, index) => Mock.Of<IContent>(
                m =>
                    m.Id == (int)xmlElement.Attribute("id") &&
                    // have every second one published and include the special one
                    m.Published == (ExamineDemoDataContentService.ProtectedNode == (int)xmlElement.Attribute("id") ||
                                    (index % 2 == 0 ? true : false)) &&
                    m.ParentId == (int)xmlElement.Attribute("parentID") &&
                    m.Level == (int)xmlElement.Attribute("level") &&
                    m.CreatorId == 0 &&
                    m.SortOrder == (int)xmlElement.Attribute("sortOrder") &&
                    m.CreateDate == (DateTime)xmlElement.Attribute("createDate") &&
                    m.UpdateDate == (DateTime)xmlElement.Attribute("updateDate") &&
                    m.Name == (string)xmlElement.Attribute(UmbracoExamineFieldNames.NodeNameFieldName) &&
                    m.GetCultureName(It.IsAny<string>()) ==
                    (string)xmlElement.Attribute(UmbracoExamineFieldNames.NodeNameFieldName) &&
                    m.Path == (string)xmlElement.Attribute("path") &&
                    m.Properties == new PropertyCollection() &&
                    m.ContentType == Mock.Of<ISimpleContentType>(mt =>
                        mt.Icon == "test" &&
                        mt.Alias == xmlElement.Name.LocalName &&
                        mt.Id == (int)xmlElement.Attribute("nodeType"))))
            .ToArray();


        return Mock.Of<IContentService>(
            x => x.GetPagedDescendants(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out longTotalRecs, It.IsAny<IQuery<IContent>>(), It.IsAny<Ordering>())
                 == allRecs);
    }

    public IUserService GetMockUserService() => Mock.Of<IUserService>(x =>
        x.GetProfileById(It.IsAny<int>()) == Mock.Of<IProfile>(p => p.Id == 0 && p.Name == "admin"));

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
                    m.GetCultureName(It.IsAny<string>()) ==
                    (string)x.Attribute(UmbracoExamineFieldNames.NodeNameFieldName) &&
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
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out totalRecs,
                It.IsAny<IQuery<IMedia>>(),
                It.IsAny<Ordering>()))
            .Returns(() => allRecs);

        //mediaServiceMock.Setup(service => service.GetPagedXmlEntries(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), out longTotalRecs))
        //    .Returns(() => allRecs.Select(x => x.ToXml()));

        return mediaServiceMock.Object;
    }

    public ILocalizationService GetMockLocalizationService() =>
        Mock.Of<ILocalizationService>(x => x.GetAllLanguages() == Array.Empty<ILanguage>());

    public static IMediaTypeService GetMockMediaTypeService(IShortStringHelper shortStringHelper)
    {
        var mediaTypeServiceMock = new Mock<IMediaTypeService>();
        mediaTypeServiceMock.Setup(x => x.GetAll())
            .Returns(new List<IMediaType>
            {
                new MediaType(shortStringHelper, -1)
                {
                    Alias = "Folder", Name = "Folder", Id = 1031, Icon = "icon-folder"
                },
                new MediaType(shortStringHelper, -1)
                {
                    Alias = "Image", Name = "Image", Id = 1032, Icon = "icon-picture"
                }
            });
        return mediaTypeServiceMock.Object;
    }

    public IProfilingLogger GetMockProfilingLogger() =>
        new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>());

    public UmbracoContentIndex GetUmbracoIndexer(
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState,
        Directory luceneDir,
        Analyzer analyzer = null,
        ILocalizationService languageService = null,
        IContentValueSetValidator validator = null)
    {
        if (languageService == null)
        {
            languageService = GetMockLocalizationService();
        }

        if (analyzer == null)
        {
            analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
        }

        if (validator == null)
        {
            validator = new ContentValueSetValidator(true);
        }

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

    public static IOptionsMonitor<LuceneDirectoryIndexOptions> GetOptions(string indexName, LuceneDirectoryIndexOptions options)
        => Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(indexName) == options);

    internal void IndexingError(object sender, IndexingErrorEventArgs e) =>
        throw new ApplicationException(e.Message, e.Exception);
}
