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
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common;
using Umbraco.Extensions;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing.Objects;
using Umbraco.Web.Composing;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class NuCacheTests
    {
        private IPublishedSnapshotService _snapshotService;
        private IVariationContextAccessor _variationAccesor;
        private ContentType _contentType;
        private PropertyType _propertyType;

        [TearDown]
        public void Teardown()
        {
            _snapshotService?.Dispose();
        }

        private void Init()
        {
            var factory = Mock.Of<IServiceProvider>();
            Current.Factory = factory;

            var publishedModelFactory = new NoopPublishedModelFactory();
            Mock.Get(factory).Setup(x => x.GetService(typeof(IPublishedModelFactory))).Returns(publishedModelFactory);
            Mock.Get(factory).Setup(x => x.GetService(typeof(IPublishedValueFallback))).Returns(new NoopPublishedValueFallback());

            // create a content node kit
            var kit = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(1, Guid.NewGuid(), 0, "-1,1", 0, -1, DateTime.Now, 0),
                DraftData = new ContentData
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
                },
                PublishedData = new ContentData
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
                }
            };

            // create a data source for NuCache
            var dataSource = new TestDataSource(kit);

            var runtime = Mock.Of<IRuntimeState>();
            Mock.Get(runtime).Setup(x => x.Level).Returns(RuntimeLevel.Run);

            var serializer = new ConfigurationEditorJsonSerializer();

            // create data types, property types and content types
            var dataType = new DataType(new VoidEditor("Editor", Mock.Of<IDataValueEditorFactory>()), serializer) { Id = 3 };

            var dataTypes = new[]
            {
                dataType
            };

            _propertyType = new PropertyType(TestHelper.ShortStringHelper, "Umbraco.Void.Editor", ValueStorageType.Nvarchar) { Alias = "prop", DataTypeId = 3, Variations = ContentVariation.Culture };
            _contentType = new ContentType(TestHelper.ShortStringHelper, -1) { Id = 2, Alias = "alias-ct", Variations = ContentVariation.Culture };
            _contentType.AddPropertyType(_propertyType);

            var contentTypes = new[]
            {
                _contentType
            };

            var contentTypeService = new Mock<IContentTypeService>();
            contentTypeService.Setup(x => x.GetAll()).Returns(contentTypes);
            contentTypeService.Setup(x => x.GetAll(It.IsAny<int[]>())).Returns(contentTypes);

            var mediaTypeService = new Mock<IMediaTypeService>();
            mediaTypeService.Setup(x => x.GetAll()).Returns(Enumerable.Empty<IMediaType>());
            mediaTypeService.Setup(x => x.GetAll(It.IsAny<int[]>())).Returns(Enumerable.Empty<IMediaType>());

            var contentTypeServiceBaseFactory = new Mock<IContentTypeBaseServiceProvider>();
            contentTypeServiceBaseFactory.Setup(x => x.For(It.IsAny<IContentBase>())).Returns(contentTypeService.Object);

            var dataTypeService = Mock.Of<IDataTypeService>();
            Mock.Get(dataTypeService).Setup(x => x.GetAll()).Returns(dataTypes);

            // create a service context
            var serviceContext = ServiceContext.CreatePartial(
                dataTypeService: dataTypeService,
                memberTypeService: Mock.Of<IMemberTypeService>(),
                memberService: Mock.Of<IMemberService>(),
                contentTypeService: contentTypeService.Object,
                mediaTypeService: mediaTypeService.Object,
                localizationService: Mock.Of<ILocalizationService>(),
                domainService: Mock.Of<IDomainService>()
            );

            // create a scope provider
            var scopeProvider = Mock.Of<IScopeProvider>();
            Mock.Get(scopeProvider)
                .Setup(x => x.CreateScope(
                    It.IsAny<IsolationLevel>(),
                    It.IsAny<RepositoryCacheMode>(),
                    It.IsAny<IEventDispatcher>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(Mock.Of<IScope>);

            // create a published content type factory
            var contentTypeFactory = new PublishedContentTypeFactory(
                publishedModelFactory,
                new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()),
                dataTypeService);

            // create a variation accessor
            _variationAccesor = new TestVariationContextAccessor();

            var typeFinder = TestHelper.GetTypeFinder();

            var globalSettings = new GlobalSettings();
            var nuCacheSettings = new NuCacheSettings();

            // at last, create the complete NuCache snapshot service!
            var options = new PublishedSnapshotServiceOptions { IgnoreLocalDb = true };
            _snapshotService = new PublishedSnapshotService(
                options,
                null,
                serviceContext,
                contentTypeFactory,
                new TestPublishedSnapshotAccessor(),
                _variationAccesor,
                Mock.Of<IProfilingLogger>(),
                NullLoggerFactory.Instance,
                scopeProvider,
                dataSource,
                new TestDefaultCultureAccessor(),
                Microsoft.Extensions.Options.Options.Create(globalSettings),
                Mock.Of<IEntityXmlSerializer>(),
                publishedModelFactory,
                TestHelper.GetHostingEnvironment(),
                Microsoft.Extensions.Options.Options.Create(nuCacheSettings));

            // invariant is the current default
            _variationAccesor.VariationContext = new VariationContext();

            Mock.Get(factory).Setup(x => x.GetService(typeof(IVariationContextAccessor))).Returns(_variationAccesor);
        }

        [Test]
        public void StandaloneVariations()
        {
            // this test implements a full standalone NuCache (based upon a test IDataSource, does not
            // use any local db files, does not rely on any database) - and tests variations

            Init();

            // get a snapshot, get a published content
            var snapshot = _snapshotService.CreatePublishedSnapshot(previewToken: null);
            var publishedContent = snapshot.Content.GetById(1);

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
        public void IsDraftIsPublished()
        {
            Init();

            // get the published published content
            var s = _snapshotService.CreatePublishedSnapshot(null);
            var c1 = s.Content.GetById(1);

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
            var c2 = s.Content.GetById(true, 1);

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
