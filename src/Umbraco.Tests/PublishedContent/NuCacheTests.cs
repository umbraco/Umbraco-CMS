using System;
using System.Collections.Generic;
using System.Data;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing.Objects;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.PublishedCache.NuCache.DataSource;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class NuCacheTests
    {
        [Test]
        public void StandaloneVariations()
        {
            // this test implements a full standalone NuCache (based upon a test IDataSource, does not
            // use any local db files, does not rely on any database) - and tests variations

            SettingsForTests.ConfigureSettings(SettingsForTests.GenerateMockUmbracoSettings());
            var globalSettings = UmbracoConfig.For.GlobalSettings();

            // create a content node kit
            var kit = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(1, Guid.NewGuid(), 0, "-1,1", 0, -1, DateTime.Now, 0),
                DraftData = new ContentData { Name="It Works2!", Published = false, TemplateId = 0, VersionId = 2, VersionDate = DateTime.Now, WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]> { { "prop", new[]
                    {
                        new PropertyData { Culture = "", Segment = "", Value = "val2" },
                        new PropertyData { Culture = "fr-FR", Segment = "", Value = "val-fr2" },
                        new PropertyData { Culture = "en-UK", Segment = "", Value = "val-uk2" }
                    } } },
                    CultureInfos = new Dictionary<string, CultureVariation>
                    {
                        { "fr-FR", new CultureVariation { Name = "name-fr2", Date = new DateTime(2018, 01, 03, 01, 00, 00) } },
                        { "en-UK", new CultureVariation { Name = "name-uk2", Date = new DateTime(2018, 01, 04, 01, 00, 00) } }
                    }
                },
                PublishedData = new ContentData { Name="It Works1!", Published = true, TemplateId = 0, VersionId = 1, VersionDate = DateTime.Now, WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]> { { "prop", new[]
                    {
                        new PropertyData { Culture = "", Segment = "", Value = "val1" },
                        new PropertyData { Culture = "fr-FR", Segment = "", Value = "val-fr1" },
                        new PropertyData { Culture = "en-UK", Segment = "", Value = "val-uk1" }
                    } } },
                    CultureInfos = new Dictionary<string, CultureVariation>
                    {
                        { "fr-FR", new CultureVariation { Name = "name-fr1", Date = new DateTime(2018, 01, 01, 01, 00, 00) } },
                        { "en-UK", new CultureVariation { Name = "name-uk1", Date = new DateTime(2018, 01, 02, 01, 00, 00) } }
                    }
                }
            };

            // create a data source for NuCache
            var dataSource = new TestDataSource(kit);

            var runtime = Mock.Of<IRuntimeState>();
            Mock.Get(runtime).Setup(x => x.Level).Returns(RuntimeLevel.Run);

            // create data types, property types and content types
            var dataType = new DataType(new VoidEditor("Editor", Mock.Of<ILogger>())) { Id = 3 };

            var dataTypes = new[]
            {
                dataType
            };

            var propertyType = new PropertyType("Umbraco.Void.Editor", ValueStorageType.Nvarchar) { Alias = "prop", DataTypeId = 3, Variations = ContentVariation.Culture };
            var contentType = new ContentType(-1) { Id = 2, Alias = "alias-ct", Variations = ContentVariation.Culture };
            contentType.AddPropertyType(propertyType);

            var contentTypes = new[]
            {
                contentType
            };

            var contentTypeService = Mock.Of<IContentTypeService>();
            Mock.Get(contentTypeService).Setup(x => x.GetAll()).Returns(contentTypes);
            Mock.Get(contentTypeService).Setup(x => x.GetAll(It.IsAny<int[]>())).Returns(contentTypes);

            var dataTypeService = Mock.Of<IDataTypeService>();
            Mock.Get(dataTypeService).Setup(x => x.GetAll()).Returns(dataTypes);

            // create a service context
            var serviceContext = new ServiceContext(
                dataTypeService : dataTypeService,
                memberTypeService: Mock.Of<IMemberTypeService>(),
                memberService: Mock.Of<IMemberService>(),
                contentTypeService : contentTypeService,
                localizationService: Mock.Of<ILocalizationService>()
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
                Mock.Of<IPublishedModelFactory>(),
                new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()),
                dataTypeService);

            // create a variation accessor
            var variationAccessor = new TestVariationContextAccessor();

            // at last, create the complete NuCache snapshot service!
            var options = new PublishedSnapshotService.Options { IgnoreLocalDb = true };
            var snapshotService = new PublishedSnapshotService(options,
                null,
                runtime,
                serviceContext,
                contentTypeFactory,
                null,
                new TestPublishedSnapshotAccessor(),
                variationAccessor,
                Mock.Of<ILogger>(),
                scopeProvider,
                Mock.Of<IDocumentRepository>(),
                Mock.Of<IMediaRepository>(),
                Mock.Of<IMemberRepository>(),
                new TestDefaultCultureAccessor(),
                dataSource,
                globalSettings,
                new SiteDomainHelper());

            // get a snapshot, get a published content
            var snapshot = snapshotService.CreatePublishedSnapshot(previewToken: null);
            var publishedContent = snapshot.Content.GetById(1);

            // invariant is the current default
            variationAccessor.VariationContext = new VariationContext();

            Assert.IsNotNull(publishedContent);
            Assert.AreEqual("It Works1!", publishedContent.Name);
            Assert.AreEqual("val1", publishedContent.Value<string>("prop"));
            Assert.AreEqual("val-fr1", publishedContent.Value<string>("prop", "fr-FR"));
            Assert.AreEqual("val-uk1", publishedContent.Value<string>("prop", "en-UK"));

            Assert.AreEqual("name-fr1", publishedContent.GetCulture("fr-FR").Name);
            Assert.AreEqual("name-uk1", publishedContent.GetCulture("en-UK").Name);

            var draftContent = snapshot.Content.GetById(true, 1);
            Assert.AreEqual("It Works2!", draftContent.Name);
            Assert.AreEqual("val2", draftContent.Value<string>("prop"));
            Assert.AreEqual("val-fr2", draftContent.Value<string>("prop", "fr-FR"));
            Assert.AreEqual("val-uk2", draftContent.Value<string>("prop", "en-UK"));

            Assert.AreEqual("name-fr2", draftContent.GetCulture("fr-FR").Name);
            Assert.AreEqual("name-uk2", draftContent.GetCulture("en-UK").Name);

            // now french is default
            variationAccessor.VariationContext = new VariationContext("fr-FR");
            Assert.AreEqual("val-fr1", publishedContent.Value<string>("prop"));
            Assert.AreEqual("name-fr1", publishedContent.GetCulture().Name);
            Assert.AreEqual("name-fr1", publishedContent.Name);
            Assert.AreEqual(new DateTime(2018, 01, 01, 01, 00, 00), publishedContent.GetCulture().Date);

            // now uk is default
            variationAccessor.VariationContext = new VariationContext("en-UK");
            Assert.AreEqual("val-uk1", publishedContent.Value<string>("prop"));
            Assert.AreEqual("name-uk1", publishedContent.GetCulture().Name);
            Assert.AreEqual("name-uk1", publishedContent.Name);
            Assert.AreEqual(new DateTime(2018, 01, 02, 01, 00, 00), publishedContent.GetCulture().Date);

            // invariant needs to be retrieved explicitely, when it's not default
            Assert.AreEqual("val1", publishedContent.Value<string>("prop", culture: ""));

            // but,
            // if the content type / property type does not vary, then it's all invariant again
            // modify the content type and property type, notify the snapshot service
            contentType.Variations = ContentVariation.Nothing;
            propertyType.Variations = ContentVariation.Nothing;
            snapshotService.Notify(new[] { new ContentTypeCacheRefresher.JsonPayload("IContentType", publishedContent.ContentType.Id, ContentTypeChangeTypes.RefreshMain) });

            // get a new snapshot (nothing changed in the old one), get the published content again
            var anotherSnapshot = snapshotService.CreatePublishedSnapshot(previewToken: null);
            var againContent = anotherSnapshot.Content.GetById(1);

            Assert.AreEqual(ContentVariation.Nothing, againContent.ContentType.Variations);
            Assert.AreEqual(ContentVariation.Nothing, againContent.ContentType.GetPropertyType("prop").Variations);

            // now, "no culture" means "invariant"
            Assert.AreEqual("It Works1!", againContent.Name);
            Assert.AreEqual("val1", againContent.Value<string>("prop"));
        }
    }
}
