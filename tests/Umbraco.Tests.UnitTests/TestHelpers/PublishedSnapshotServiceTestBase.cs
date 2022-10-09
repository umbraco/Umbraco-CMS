using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common;
using Umbraco.Extensions;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Cms.Tests.UnitTests.TestHelpers;

[TestFixture]
public class PublishedSnapshotServiceTestBase
{
    [SetUp]
    public virtual void Setup()
    {
        VariationContextAccessor = new TestVariationContextAccessor();
        PublishedSnapshotAccessor = new TestPublishedSnapshotAccessor();
    }

    [TearDown]
    public void Teardown() => SnapshotService?.Dispose();

    protected IShortStringHelper ShortStringHelper { get; } = TestHelper.ShortStringHelper;

    protected virtual IPublishedModelFactory PublishedModelFactory { get; } = new NoopPublishedModelFactory();

    protected IContentTypeService ContentTypeService { get; private set; }

    protected IMediaTypeService MediaTypeService { get; private set; }

    protected IDataTypeService DataTypeService { get; private set; }

    protected IDomainService DomainService { get; private set; }

    protected IPublishedValueFallback PublishedValueFallback { get; private set; }

    protected IPublishedSnapshotService SnapshotService { get; private set; }

    protected IVariationContextAccessor VariationContextAccessor { get; private set; }

    protected TestPublishedSnapshotAccessor PublishedSnapshotAccessor { get; private set; }

    protected TestNuCacheContentService NuCacheContentService { get; private set; }

    protected PublishedContentTypeFactory PublishedContentTypeFactory { get; private set; }

    protected GlobalSettings GlobalSettings { get; } = new();

    protected virtual PropertyValueConverterCollection PropertyValueConverterCollection =>
        new(() => new[] { new TestSimpleTinyMceValueConverter() });

    protected IPublishedContent GetContent(int id)
    {
        var snapshot = GetPublishedSnapshot();
        var doc = snapshot.Content.GetById(id);
        Assert.IsNotNull(doc);
        return doc;
    }

    protected IPublishedContent GetMedia(int id)
    {
        var snapshot = GetPublishedSnapshot();
        var doc = snapshot.Media.GetById(id);
        Assert.IsNotNull(doc);
        return doc;
    }

    protected UrlProvider GetUrlProvider(
        IUmbracoContextAccessor umbracoContextAccessor,
        RequestHandlerSettings requestHandlerSettings,
        WebRoutingSettings webRoutingSettings,
        out UriUtility uriUtility)
    {
        uriUtility = new UriUtility(Mock.Of<IHostingEnvironment>());
        var urlProvider = new DefaultUrlProvider(
            Mock.Of<IOptionsMonitor<RequestHandlerSettings>>(x => x.CurrentValue == requestHandlerSettings),
            Mock.Of<ILogger<DefaultUrlProvider>>(),
            new SiteDomainMapper(),
            umbracoContextAccessor,
            uriUtility,
            Mock.Of<ILocalizationService>(x => x.GetDefaultLanguageIsoCode() == GlobalSettings.DefaultUILanguage));

        var publishedUrlProvider = new UrlProvider(
            umbracoContextAccessor,
            Options.Create(webRoutingSettings),
            new UrlProviderCollection(() => new[] { urlProvider }),
            new MediaUrlProviderCollection(() => Enumerable.Empty<IMediaUrlProvider>()),
            Mock.Of<IVariationContextAccessor>());

        return publishedUrlProvider;
    }

    protected static PublishedRouter CreatePublishedRouter(
        IUmbracoContextAccessor umbracoContextAccessor,
        IEnumerable<IContentFinder> contentFinders = null,
        IPublishedUrlProvider publishedUrlProvider = null) => new(
        Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == new WebRoutingSettings()),
        new ContentFinderCollection(() => contentFinders ?? Enumerable.Empty<IContentFinder>()),
        new TestLastChanceFinder(),
        new TestVariationContextAccessor(),
        Mock.Of<IProfilingLogger>(),
        Mock.Of<ILogger<PublishedRouter>>(),
        publishedUrlProvider ?? Mock.Of<IPublishedUrlProvider>(),
        Mock.Of<IRequestAccessor>(),
        Mock.Of<IPublishedValueFallback>(),
        Mock.Of<IFileService>(),
        Mock.Of<IContentTypeService>(),
        umbracoContextAccessor,
        Mock.Of<IEventAggregator>());

    protected IUmbracoContextAccessor GetUmbracoContextAccessor(string urlAsString)
    {
        var snapshot = GetPublishedSnapshot();

        var uri = new Uri(urlAsString.Contains(Uri.SchemeDelimiter)
            ? urlAsString
            : $"http://example.com{urlAsString}");

        var umbracoContext = Mock.Of<IUmbracoContext>(
            x => x.CleanedUmbracoUrl == uri
                 && x.Content == snapshot.Content
                 && x.PublishedSnapshot == snapshot);
        var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
        return umbracoContextAccessor;
    }

    /// <summary>
    ///     Used as a property editor for any test property that has an editor alias called "Umbraco.Void.RTE"
    /// </summary>
    private class TestSimpleTinyMceValueConverter : SimpleTinyMceValueConverter
    {
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias == "Umbraco.Void.RTE";
    }

    protected static DataType[] GetDefaultDataTypes()
    {
        var serializer = new ConfigurationEditorJsonSerializer();

        // create data types, property types and content types
        var dataType =
            new DataType(new VoidEditor("Editor", Mock.Of<IDataValueEditorFactory>()), serializer) { Id = 3 };

        return new[] { dataType };
    }

    protected virtual ServiceContext CreateServiceContext(IContentType[] contentTypes, IMediaType[] mediaTypes, IDataType[] dataTypes)
    {
        var contentTypeService = new Mock<IContentTypeService>();
        contentTypeService.Setup(x => x.GetAll()).Returns(contentTypes);
        contentTypeService.Setup(x => x.GetAll(It.IsAny<int[]>())).Returns(contentTypes);
        contentTypeService.Setup(x => x.Get(It.IsAny<string>()))
            .Returns((string alias) => contentTypes.FirstOrDefault(x => x.Alias.InvariantEquals(alias)));

        var mediaTypeService = new Mock<IMediaTypeService>();
        mediaTypeService.Setup(x => x.GetAll()).Returns(mediaTypes);
        mediaTypeService.Setup(x => x.GetAll(It.IsAny<int[]>())).Returns(mediaTypes);
        mediaTypeService.Setup(x => x.Get(It.IsAny<string>()))
            .Returns((string alias) => mediaTypes.FirstOrDefault(x => x.Alias.InvariantEquals(alias)));

        var contentTypeServiceBaseFactory = new Mock<IContentTypeBaseServiceProvider>();
        contentTypeServiceBaseFactory.Setup(x => x.For(It.IsAny<IContentBase>()))
            .Returns(contentTypeService.Object);

        var dataTypeServiceMock = new Mock<IDataTypeService>();
        dataTypeServiceMock.Setup(x => x.GetAll()).Returns(dataTypes);

        return ServiceContext.CreatePartial(
            dataTypeService: dataTypeServiceMock.Object,
            memberTypeService: Mock.Of<IMemberTypeService>(),
            memberService: Mock.Of<IMemberService>(),
            contentTypeService: contentTypeService.Object,
            mediaTypeService: mediaTypeService.Object,
            localizationService: Mock.Of<ILocalizationService>(),
            domainService: Mock.Of<IDomainService>(),
            fileService: Mock.Of<IFileService>());
    }

    /// <summary>
    ///     Creates a published snapshot and set the accessor to resolve the created one
    /// </summary>
    /// <returns></returns>
    protected IPublishedSnapshot GetPublishedSnapshot()
    {
        var snapshot = SnapshotService.CreatePublishedSnapshot(null);
        PublishedSnapshotAccessor.SetCurrent(snapshot);
        return snapshot;
    }

    /// <summary>
    ///     Initializes the <see cref="IPublishedSnapshotService'" /> with a source of data
    /// </summary>
    protected void InitializedCache(
        IEnumerable<ContentNodeKit> contentNodeKits,
        IContentType[] contentTypes,
        IDataType[] dataTypes = null,
        IEnumerable<ContentNodeKit> mediaNodeKits = null,
        IMediaType[] mediaTypes = null)
    {
        // create a data source for NuCache
        NuCacheContentService = new TestNuCacheContentService(contentNodeKits, mediaNodeKits);

        var runtime = Mock.Of<IRuntimeState>();
        Mock.Get(runtime).Setup(x => x.Level).Returns(RuntimeLevel.Run);

        // create a service context
        var serviceContext = CreateServiceContext(
            contentTypes ?? Array.Empty<IContentType>(),
            mediaTypes ?? Array.Empty<IMediaType>(),
            dataTypes ?? GetDefaultDataTypes());

        DataTypeService = serviceContext.DataTypeService;
        ContentTypeService = serviceContext.ContentTypeService;
        MediaTypeService = serviceContext.MediaTypeService;
        DomainService = serviceContext.DomainService;

        // create a scope provider
        var scopeProvider = Mock.Of<IScopeProvider>();
        Mock.Get(scopeProvider)
            .Setup(x => x.CreateScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<IScope>);

        // create a published content type factory
        PublishedContentTypeFactory = new PublishedContentTypeFactory(
            PublishedModelFactory,
            PropertyValueConverterCollection,
            DataTypeService);

        var typeFinder = TestHelper.GetTypeFinder();

        var nuCacheSettings = new NuCacheSettings();

        // at last, create the complete NuCache snapshot service!
        var options = new PublishedSnapshotServiceOptions { IgnoreLocalDb = true };
        SnapshotService = new PublishedSnapshotService(
            options,
            Mock.Of<ISyncBootStateAccessor>(x => x.GetSyncBootState() == SyncBootState.WarmBoot),
            new SimpleMainDom(),
            serviceContext,
            PublishedContentTypeFactory,
            PublishedSnapshotAccessor,
            VariationContextAccessor,
            Mock.Of<IProfilingLogger>(),
            NullLoggerFactory.Instance,
            scopeProvider,
            NuCacheContentService,
            new TestDefaultCultureAccessor(),
            Options.Create(GlobalSettings),
            PublishedModelFactory,
            TestHelper.GetHostingEnvironment(),
            Options.Create(nuCacheSettings),
            new ContentDataSerializer(new DictionaryOfPropertyDataSerializer()));

        // invariant is the current default
        VariationContextAccessor.VariationContext = new VariationContext();

        PublishedValueFallback = new PublishedValueFallback(serviceContext, VariationContextAccessor);
    }
}
