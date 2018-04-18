using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class PublishedContentOtherTests // FIXME rename!
    {
        [Test]
        public void Test()
        {
            SettingsForTests.ConfigureSettings(SettingsForTests.GenerateMockUmbracoSettings());
            var globalSettings = UmbracoConfig.For.GlobalSettings();

            // fixme - missing variant names here, and what else?
            var kit = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(1, Guid.NewGuid(), 0, "-1,1", 0, -1, DateTime.Now, 0),
                DraftData = new ContentData { Name="It Works2!", Published = false, TemplateId = 0, VersionId = 2, VersionDate = DateTime.Now, WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]> { { "prop", new[]
                    {
                        new PropertyData { Value = "val2" },
                        new PropertyData { Culture = "fr-FR", Value = "val-fr2" },
                        new PropertyData { Culture = "en-UK", Value = "val-uk2" }
                    } } } },
                PublishedData = new ContentData { Name="It Works1!", Published = true, TemplateId = 0, VersionId = 1, VersionDate = DateTime.Now, WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]> { { "prop", new[]
                    {
                        new PropertyData { Value = "val1" },
                        new PropertyData { Culture = "fr-FR", Value = "val-fr1" },
                        new PropertyData { Culture = "en-UK", Value = "val-uk1" }
                    } } } }
            };

            var dataSource = new TestDataSource(kit);

            var runtime = Mock.Of<IRuntimeState>();
            Mock.Get(runtime).Setup(x => x.Level).Returns(RuntimeLevel.Run);

            var propertyType = new PropertyType("Umbraco.Void.Editor", ValueStorageType.Nvarchar) { Alias = "prop", DataTypeId = 3, Variations = ContentVariation.InvariantNeutral | ContentVariation.CultureNeutral };
            var contentType = new ContentType(-1) { Id = 2, Alias = "alias-ct", Variations = ContentVariation.InvariantNeutral | ContentVariation.CultureNeutral };
            contentType.AddPropertyType(propertyType);

            var contentTypes = new[]
            {
                contentType
            };

            var dataType = new DataType(new VoidEditor("Editor", Mock.Of<ILogger>())) { Id = 3 };

            var dataTypes = new[]
            {
                dataType
            };

            var contentTypeService = Mock.Of<IContentTypeService>();
            Mock.Get(contentTypeService).Setup(x => x.GetAll()).Returns(contentTypes);

            var dataTypeService = Mock.Of<IDataTypeService>();
            Mock.Get(dataTypeService).Setup(x => x.GetAll()).Returns(dataTypes);

            var serviceContext = new ServiceContext(
                dataTypeService : dataTypeService,

                memberTypeService: Mock.Of<IMemberTypeService>(),
                memberService: Mock.Of<IMemberService>(),

                contentTypeService : contentTypeService,

                localizationService: Mock.Of<ILocalizationService>()
            );

            var contentTypeFactory = new PublishedContentTypeFactory(
                Mock.Of<IPublishedModelFactory>(),
                new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()),
                dataTypeService);

            var documentRepository = Mock.Of<IDocumentRepository>();
            var mediaRepository = Mock.Of<IMediaRepository>();
            var memberRepository = Mock.Of<IMemberRepository>();

            var snapshotAccessor = new TestPublishedSnapshotAccessor();

            var scopeProvider = Mock.Of<IScopeProvider>();
            Mock.Get(scopeProvider)
                .Setup(x => x.CreateScope(
                    It.IsAny<IsolationLevel>(),
                    It.IsAny<RepositoryCacheMode>(),
                    It.IsAny<IEventDispatcher>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(() => Mock.Of<IScope>());

            var variationAccessor = new TestPublishedVariationContextAccessor();

            var options = new PublishedSnapshotService.Options { IgnoreLocalDb = true };
            var snapshotService = new PublishedSnapshotService(options,
                null,
                runtime,
                serviceContext,
                contentTypeFactory,
                null,
                snapshotAccessor,
                variationAccessor,
                Mock.Of<ILogger>(),
                scopeProvider,
                documentRepository,
                mediaRepository,
                memberRepository,
                dataSource,
                globalSettings);

            var snapshot = snapshotService.CreatePublishedSnapshot(previewToken: null);
            var publishedContent = snapshot.Content.GetById(1);

            // invariant is the current default
            variationAccessor.Context = null;

            Assert.IsNotNull(publishedContent);
            Assert.AreEqual("It Works1!", publishedContent.Name);
            Assert.AreEqual("val1", publishedContent.Value<string>("prop"));
            Assert.AreEqual("val-fr1", publishedContent.Value<string>("prop", "fr-FR"));
            Assert.AreEqual("val-uk1", publishedContent.Value<string>("prop", "en-UK"));

            var draftContent = snapshot.Content.GetById(true, 1);
            Assert.AreEqual("It Works2!", draftContent.Name);
            Assert.AreEqual("val2", draftContent.Value<string>("prop"));
            Assert.AreEqual("val-fr2", draftContent.Value<string>("prop", "fr-FR"));
            Assert.AreEqual("val-uk2", draftContent.Value<string>("prop", "en-UK"));

            variationAccessor.Context = new PublishedVariationContext("fr-FR");
            Assert.AreEqual("val-fr1", publishedContent.Value<string>("prop"));
            variationAccessor.Context = new PublishedVariationContext("en-UK");
            Assert.AreEqual("val-uk1", publishedContent.Value<string>("prop"));

            // invariant needs to be retrieved explicitely, when it's not default
            Assert.AreEqual("val1", publishedContent.Value<string>("prop", culture: null));

            // then, test fallback
        }

        internal class TestDataSource : IDataSource
        {
            private readonly Dictionary<int, ContentNodeKit> _kits;

            public TestDataSource(params ContentNodeKit[] kits)
                : this((IEnumerable<ContentNodeKit>) kits)
            { }

            public TestDataSource(IEnumerable<ContentNodeKit> kits)
            {
                _kits = kits.ToDictionary(x => x.Node.Id, x => x);
            }

            public ContentNodeKit GetContentSource(IScope scope, int id)
                => _kits.TryGetValue(id, out var kit) ? kit : default;

            public IEnumerable<ContentNodeKit> GetAllContentSources(IScope scope)
                => _kits.Values;

            public IEnumerable<ContentNodeKit> GetBranchContentSources(IScope scope, int id)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ContentNodeKit> GetTypeContentSources(IScope scope, IEnumerable<int> ids)
                => _kits.Values.Where(x => ids.Contains(x.ContentTypeId));

            public ContentNodeKit GetMediaSource(IScope scope, int id)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ContentNodeKit> GetAllMediaSources(IScope scope)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ContentNodeKit> GetBranchMediaSources(IScope scope, int id)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ContentNodeKit> GetTypeMediaSources(IScope scope, IEnumerable<int> ids)
            {
                throw new NotImplementedException();
            }
        }
    }
}
