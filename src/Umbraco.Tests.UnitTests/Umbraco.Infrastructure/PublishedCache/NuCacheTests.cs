using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache
{
    [TestFixture]
    public class NuCacheTests
    {
        private IPublishedSnapshotService _snapshotService;
        private IVariationContextAccessor _variationAccesor;
        private IContentCacheDataSerializerFactory _contentNestedDataSerializerFactory;
        private ContentType _contentType;
        private PropertyType _propertyType;
        private TestPublishedSnapshotAccessor _publishedSnapshotAccessor;

        [SetUp]
        public void Setup()
        {

        }

        [TearDown]
        public void Teardown()
        {
            _snapshotService?.Dispose();
        }

        private ContentNodeKit CreateKit()
        {
            // create a content node kit
            var contentNode = new ContentNode(1, Guid.NewGuid(), 0, "-1,1", 0, -1, DateTime.Now, 0);
            var draftData = new ContentData
            {
                Name = "It Works2!",
                Published = false,
                TemplateId = 0,
                VersionId = 2,
                VersionDate = DateTime.Now,
                WriterId = 0,
                Properties = new Dictionary<string, PropertyData[]> { { "prop", new[]
                    {
                        new PropertyData { Culture = "", Segment = "", Value = "val2" },
                        new PropertyData { Culture = "fr-FR", Segment = "", Value = "val-fr2" },
                        new PropertyData { Culture = "en-UK", Segment = "", Value = "val-uk2" },
                        new PropertyData { Culture = "dk-DA", Segment = "", Value = "val-da2" },
                        new PropertyData { Culture = "de-DE", Segment = "", Value = "val-de2" }
                    } } },
                CultureInfos = new Dictionary<string, CultureVariation>
                    {
                        // draft data = everything, and IsDraft indicates what's edited
                        { "fr-FR", new CultureVariation { Name = "name-fr2", IsDraft = true, Date = new DateTime(2018, 01, 03, 01, 00, 00) } },
                        { "en-UK", new CultureVariation { Name = "name-uk2", IsDraft = true, Date = new DateTime(2018, 01, 04, 01, 00, 00) } },
                        { "dk-DA", new CultureVariation { Name = "name-da2", IsDraft = true, Date = new DateTime(2018, 01, 05, 01, 00, 00) } },
                        { "de-DE", new CultureVariation { Name = "name-de1", IsDraft = false, Date = new DateTime(2018, 01, 02, 01, 00, 00) } }
                    }
            };
            var publishedData = new ContentData
            {
                Name = "It Works1!",
                Published = true,
                TemplateId = 0,
                VersionId = 1,
                VersionDate = DateTime.Now,
                WriterId = 0,
                Properties = new Dictionary<string, PropertyData[]> { { "prop", new[]
                    {
                        new PropertyData { Culture = "", Segment = "", Value = "val1" },
                        new PropertyData { Culture = "fr-FR", Segment = "", Value = "val-fr1" },
                        new PropertyData { Culture = "en-UK", Segment = "", Value = "val-uk1" }
                    } } },
                CultureInfos = new Dictionary<string, CultureVariation>
                    {
                        // published data = only what's actually published, and IsDraft has to be false
                        { "fr-FR", new CultureVariation { Name = "name-fr1", IsDraft = false, Date = new DateTime(2018, 01, 01, 01, 00, 00) } },
                        { "en-UK", new CultureVariation { Name = "name-uk1", IsDraft = false, Date = new DateTime(2018, 01, 02, 01, 00, 00) } },
                        { "de-DE", new CultureVariation { Name = "name-de1", IsDraft = false, Date = new DateTime(2018, 01, 02, 01, 00, 00) } }
                    }
            };
            var kit = new ContentNodeKit(contentNode, 2, draftData, publishedData);
            return kit;
        }

        private void Init(ContentNodeKit[] contentNodeKits)
        {
            var factory = new Mock<IServiceProvider>();

            var publishedModelFactory = new NoopPublishedModelFactory();
            factory.Setup(x => x.GetService(typeof(IPublishedModelFactory))).Returns(publishedModelFactory);
            factory.Setup(x => x.GetService(typeof(IPublishedValueFallback))).Returns(new NoopPublishedValueFallback());

            var runtime = Mock.Of<IRuntimeState>();
            Mock.Get(runtime).Setup(x => x.Level).Returns(RuntimeLevel.Run);            

            _propertyType = new PropertyType(TestHelper.ShortStringHelper, "Umbraco.Void.Editor", ValueStorageType.Nvarchar) { Alias = "prop", DataTypeId = 3, Variations = ContentVariation.Culture };
            _contentType = new ContentType(TestHelper.ShortStringHelper, -1) { Id = 2, Alias = "alias-ct", Variations = ContentVariation.Culture };
            _contentType.AddPropertyType(_propertyType);

            var contentTypes = new[]
            {
                _contentType
            };

            // create a service context
            ServiceContext serviceContext = GetServiceContext(contentTypes, out var dataTypeService);

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
            var contentTypeFactory = new PublishedContentTypeFactory(
                publishedModelFactory,
                new PropertyValueConverterCollection(() => Array.Empty<IPropertyValueConverter>()),
                dataTypeService);

            // create a variation accessor
            _variationAccesor = new TestVariationContextAccessor();

            _publishedSnapshotAccessor = new TestPublishedSnapshotAccessor();

            var typeFinder = TestHelper.GetTypeFinder();

            var globalSettings = new GlobalSettings();
            var nuCacheSettings = new NuCacheSettings();

            // create a data source for NuCache
            var nucacheContentService = new TestNuCacheContentService(contentNodeKits);
            _contentNestedDataSerializerFactory = new JsonContentNestedDataSerializerFactory();

            // at last, create the complete NuCache snapshot service!
            var options = new PublishedSnapshotServiceOptions { IgnoreLocalDb = true };
            _snapshotService = new PublishedSnapshotService(
                options,
                Mock.Of<ISyncBootStateAccessor>(x => x.GetSyncBootState() == SyncBootState.WarmBoot),
                new SimpleMainDom(),
                serviceContext,
                contentTypeFactory,
                _publishedSnapshotAccessor,
                _variationAccesor,
                Mock.Of<IProfilingLogger>(),
                NullLoggerFactory.Instance,
                scopeProvider,
                nucacheContentService,
                new TestDefaultCultureAccessor(),
                Microsoft.Extensions.Options.Options.Create(globalSettings),
                publishedModelFactory,
                TestHelper.GetHostingEnvironment(),
                Microsoft.Extensions.Options.Options.Create(nuCacheSettings),
                _contentNestedDataSerializerFactory,
                new ContentDataSerializer(new DictionaryOfPropertyDataSerializer()));

            // invariant is the current default
            _variationAccesor.VariationContext = new VariationContext();

            factory.Setup(x => x.GetService(typeof(IVariationContextAccessor))).Returns(_variationAccesor);
        }

        private static ServiceContext GetServiceContext(ContentType[] contentTypes, out IDataTypeService dataTypeService)
        {
            var serializer = new ConfigurationEditorJsonSerializer();

            // create data types, property types and content types
            var dataType = new DataType(new VoidEditor("Editor", Mock.Of<IDataValueEditorFactory>()), serializer) { Id = 3 };

            var dataTypes = new[]
            {
                dataType
            };

            var contentTypeService = new Mock<IContentTypeService>();
            contentTypeService.Setup(x => x.GetAll()).Returns(contentTypes);
            contentTypeService.Setup(x => x.GetAll(It.IsAny<int[]>())).Returns(contentTypes);

            var mediaTypeService = new Mock<IMediaTypeService>();
            mediaTypeService.Setup(x => x.GetAll()).Returns(Enumerable.Empty<IMediaType>());
            mediaTypeService.Setup(x => x.GetAll(It.IsAny<int[]>())).Returns(Enumerable.Empty<IMediaType>());

            var contentTypeServiceBaseFactory = new Mock<IContentTypeBaseServiceProvider>();
            contentTypeServiceBaseFactory.Setup(x => x.For(It.IsAny<IContentBase>())).Returns(contentTypeService.Object);

            var dataTypeServiceMock = new Mock<IDataTypeService>();
            dataTypeServiceMock.Setup(x => x.GetAll()).Returns(dataTypes);
            dataTypeService = dataTypeServiceMock.Object;

            return ServiceContext.CreatePartial(
                dataTypeService: dataTypeService,
                memberTypeService: Mock.Of<IMemberTypeService>(),
                memberService: Mock.Of<IMemberService>(),
                contentTypeService: contentTypeService.Object,
                mediaTypeService: mediaTypeService.Object,
                localizationService: Mock.Of<ILocalizationService>(),
                domainService: Mock.Of<IDomainService>()
                );
        }

        /// <summary>
        /// Creates a published snapshot and set the accessor to resolve the created one
        /// </summary>
        /// <returns></returns>
        private IPublishedSnapshot GetPublishedSnapshot()
        {
            var snapshot = _snapshotService.CreatePublishedSnapshot(previewToken: null);
            _publishedSnapshotAccessor.SetCurrent(snapshot);
            return snapshot;
        }

        [Test]
        public void GivenTestableNuCache_WhenIPublishedContentIsResolved_VariantDataIsCorrect()
        {
            // this test implements a full standalone NuCache (based upon a test IDataSource, does not
            // use any local db files, does not rely on any database) - and tests variations

            Init(new[] { CreateKit() });

            // get a snapshot, get a published content
            IPublishedSnapshot snapshot = GetPublishedSnapshot();
            IPublishedContent publishedContent = snapshot.Content.GetById(1);

            Assert.IsNotNull(publishedContent);
            Assert.AreEqual("val1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop"));
            Assert.AreEqual("val-fr1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop", "fr-FR"));
            Assert.AreEqual("val-uk1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop", "en-UK"));

            Assert.IsNull(publishedContent.Name(_variationAccesor)); // no invariant name for varying content
            Assert.AreEqual("name-fr1", publishedContent.Name(_variationAccesor, "fr-FR"));
            Assert.AreEqual("name-uk1", publishedContent.Name(_variationAccesor, "en-UK"));

            var draftContent = snapshot.Content.GetById(true, 1);
            Assert.AreEqual("val2", draftContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop"));
            Assert.AreEqual("val-fr2", draftContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop", "fr-FR"));
            Assert.AreEqual("val-uk2", draftContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop", "en-UK"));

            Assert.IsNull(draftContent.Name(_variationAccesor)); // no invariant name for varying content
            Assert.AreEqual("name-fr2", draftContent.Name(_variationAccesor, "fr-FR"));
            Assert.AreEqual("name-uk2", draftContent.Name(_variationAccesor, "en-UK"));

            // now french is default
            _variationAccesor.VariationContext = new VariationContext("fr-FR");
            Assert.AreEqual("val-fr1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop"));
            Assert.AreEqual("name-fr1", publishedContent.Name(_variationAccesor));
            Assert.AreEqual(new DateTime(2018, 01, 01, 01, 00, 00), publishedContent.CultureDate(_variationAccesor));

            // now uk is default
            _variationAccesor.VariationContext = new VariationContext("en-UK");
            Assert.AreEqual("val-uk1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop"));
            Assert.AreEqual("name-uk1", publishedContent.Name(_variationAccesor));
            Assert.AreEqual(new DateTime(2018, 01, 02, 01, 00, 00), publishedContent.CultureDate(_variationAccesor));

            // invariant needs to be retrieved explicitly, when it's not default
            Assert.AreEqual("val1", publishedContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop", culture: ""));

            // but,
            // if the content type / property type does not vary, then it's all invariant again
            // modify the content type and property type, notify the snapshot service
            _contentType.Variations = ContentVariation.Nothing;
            _propertyType.Variations = ContentVariation.Nothing;
            _snapshotService.Notify(new[] { new ContentTypeCacheRefresher.JsonPayload("IContentType", publishedContent.ContentType.Id, ContentTypeChangeTypes.RefreshMain) });

            // get a new snapshot (nothing changed in the old one), get the published content again
            var anotherSnapshot = _snapshotService.CreatePublishedSnapshot(previewToken: null);
            var againContent = anotherSnapshot.Content.GetById(1);

            Assert.AreEqual(ContentVariation.Nothing, againContent.ContentType.Variations);
            Assert.AreEqual(ContentVariation.Nothing, againContent.ContentType.GetPropertyType("prop").Variations);

            // now, "no culture" means "invariant"
            Assert.AreEqual("It Works1!", againContent.Name(_variationAccesor));
            Assert.AreEqual("val1", againContent.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop"));
        }

        [Test]
        public void GivenTestableNuCache_WhenIPublishedContentIsResolved_PublishedAndDraftDataIsCorrect()
        {
            Init(new[] { CreateKit() });

            // get the published published content
            var snapshot = GetPublishedSnapshot();
            var c1 = snapshot.Content.GetById(1);

            // published content = nothing is draft here
            Assert.IsFalse(c1.IsDraft("fr-FR"));
            Assert.IsFalse(c1.IsDraft("en-UK"));
            Assert.IsFalse(c1.IsDraft("dk-DA"));
            Assert.IsFalse(c1.IsDraft("de-DE"));

            // and only those with published name, are published
            Assert.IsTrue(c1.IsPublished("fr-FR"));
            Assert.IsTrue(c1.IsPublished("en-UK"));
            Assert.IsFalse(c1.IsDraft("dk-DA"));
            Assert.IsTrue(c1.IsPublished("de-DE"));

            // get the draft published content
            var c2 = snapshot.Content.GetById(true, 1);

            // draft content = we have drafts
            Assert.IsTrue(c2.IsDraft("fr-FR"));
            Assert.IsTrue(c2.IsDraft("en-UK"));
            Assert.IsTrue(c2.IsDraft("dk-DA"));
            Assert.IsFalse(c2.IsDraft("de-DE")); // except for the one that does not

            // and only those with published name, are published
            Assert.IsTrue(c2.IsPublished("fr-FR"));
            Assert.IsTrue(c2.IsPublished("en-UK"));
            Assert.IsFalse(c2.IsPublished("dk-DA"));
            Assert.IsTrue(c2.IsPublished("de-DE"));
        }

    }
}
