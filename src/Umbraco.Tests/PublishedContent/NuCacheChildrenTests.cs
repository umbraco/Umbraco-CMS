using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
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
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing.Objects;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web.Cache;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class NuCacheChildrenTests
    {
        private IPublishedSnapshotService _snapshotService;
        private IVariationContextAccessor _variationAccesor;
        private IPublishedSnapshotAccessor _snapshotAccessor;
        private ContentType _contentType;
        private PropertyType _propertyType;
        private TestDataSource _source;

        private void Init(IEnumerable<ContentNodeKit> kits)
        {
            Current.Reset();
            Current.UnlockConfigs();
            Current.Configs.Add(SettingsForTests.GenerateMockUmbracoSettings);
            Current.Configs.Add<IGlobalSettings>(() => new GlobalSettings());
            var globalSettings = Current.Configs.Global();

            // create a data source for NuCache
            _source = new TestDataSource(kits);

            var runtime = Mock.Of<IRuntimeState>();
            Mock.Get(runtime).Setup(x => x.Level).Returns(RuntimeLevel.Run);

            // create data types, property types and content types
            var dataType = new DataType(new VoidEditor("Editor", Mock.Of<ILogger>())) { Id = 3 };

            var dataTypes = new[]
            {
                dataType
            };

            _propertyType = new PropertyType("Umbraco.Void.Editor", ValueStorageType.Nvarchar) { Alias = "prop", DataTypeId = 3, Variations = ContentVariation.Nothing };
            _contentType = new ContentType(-1) { Id = 2, Alias = "ctype", Variations = ContentVariation.Nothing };
            _contentType.AddPropertyType(_propertyType);

            var contentTypes = new[]
            {
                _contentType
            };

            var contentTypeService = Mock.Of<IContentTypeService>();
            Mock.Get(contentTypeService).Setup(x => x.GetAll()).Returns(contentTypes);
            Mock.Get(contentTypeService).Setup(x => x.GetAll(It.IsAny<int[]>())).Returns(contentTypes);

            var contentTypeServiceBaseFactory = Mock.Of<IContentTypeBaseServiceProvider>();
            Mock.Get(contentTypeServiceBaseFactory).Setup(x => x.For(It.IsAny<IContentBase>())).Returns(contentTypeService);

            var dataTypeService = Mock.Of<IDataTypeService>();
            Mock.Get(dataTypeService).Setup(x => x.GetAll()).Returns(dataTypes);

            // create a service context
            var serviceContext = ServiceContext.CreatePartial(
                dataTypeService: dataTypeService,
                memberTypeService: Mock.Of<IMemberTypeService>(),
                memberService: Mock.Of<IMemberService>(),
                contentTypeService: contentTypeService,
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

            // create accessors
            _variationAccesor = new TestVariationContextAccessor();
            _snapshotAccessor = new TestPublishedSnapshotAccessor();

            // at last, create the complete NuCache snapshot service!
            var options = new PublishedSnapshotService.Options { IgnoreLocalDb = true };
            _snapshotService = new PublishedSnapshotService(options,
                null,
                runtime,
                serviceContext,
                contentTypeFactory,
                null,
                _snapshotAccessor,
                _variationAccesor,
                Mock.Of<ILogger>(),
                scopeProvider,
                Mock.Of<IDocumentRepository>(),
                Mock.Of<IMediaRepository>(),
                Mock.Of<IMemberRepository>(),
                new TestDefaultCultureAccessor(),
                _source,
                globalSettings,
                Mock.Of<IEntityXmlSerializer>(),
                Mock.Of<IPublishedModelFactory>(),
                new UrlSegmentProviderCollection(new[] { new DefaultUrlSegmentProvider() }));

            // invariant is the current default
            _variationAccesor.VariationContext = new VariationContext();
        }

        private IEnumerable<ContentNodeKit> GetKits()
        {
            var paths = new Dictionary<int, string> { { -1, "-1" } };

            ContentNodeKit CreateKit(int id, int parentId, int sortOrder)
            {
                if (!paths.TryGetValue(parentId, out var parentPath))
                    throw new Exception("Unknown parent.");

                var path = paths[id] = parentPath + "," + id;
                var level = path.Count(x => x == ',');

                return new ContentNodeKit
                {
                    ContentTypeId = 2,
                    Node = new ContentNode(id, Guid.NewGuid(), level, path, sortOrder, parentId, DateTime.Now, 0),
                    DraftData = null,
                    PublishedData = new ContentData
                    {
                        Name = "N" + id,
                        Published = true,
                        TemplateId = 0,
                        VersionId = 1,
                        VersionDate = DateTime.Now,
                        WriterId = 0,
                        Properties = new Dictionary<string, PropertyData[]>(),
                        CultureInfos = new Dictionary<string, CultureVariation>()
                    }
                };
            }

            yield return CreateKit(1, -1, 1);
            yield return CreateKit(2, -1, 2);
            yield return CreateKit(3, -1, 3);

            yield return CreateKit(4, 1, 1);
            yield return CreateKit(5, 1, 2);
            yield return CreateKit(6, 1, 3);

            yield return CreateKit(7, 2, 3);
            yield return CreateKit(8, 2, 2);
            yield return CreateKit(9, 2, 1);

            yield return CreateKit(10, 3, 1);

            yield return CreateKit(11, 4, 1);
            yield return CreateKit(12, 4, 2);
        }

        [Test]
        public void EmptyTest()
        {
            Init(Enumerable.Empty<ContentNodeKit>());

            var snapshot = _snapshotService.CreatePublishedSnapshot(previewToken: null);
            _snapshotAccessor.PublishedSnapshot = snapshot;

            var documents = snapshot.Content.GetAtRoot().ToArray();
            Assert.AreEqual(0, documents.Length);
        }

        [Test]
        public void ChildrenTest()
        {
            Init(GetKits());

            var snapshot = _snapshotService.CreatePublishedSnapshot(previewToken: null);
            _snapshotAccessor.PublishedSnapshot = snapshot;

            var documents = snapshot.Content.GetAtRoot().ToArray();
            AssertDocuments(documents, "N1", "N2", "N3");

            documents = snapshot.Content.GetById(1).Children().ToArray();
            AssertDocuments(documents, "N4", "N5", "N6");

            documents = snapshot.Content.GetById(2).Children().ToArray();
            AssertDocuments(documents, "N9", "N8", "N7");

            documents = snapshot.Content.GetById(3).Children().ToArray();
            AssertDocuments(documents, "N10");

            documents = snapshot.Content.GetById(4).Children().ToArray();
            AssertDocuments(documents, "N11", "N12");

            documents = snapshot.Content.GetById(10).Children().ToArray();
            AssertDocuments(documents);
        }

        [Test]
        public void ParentTest()
        {
            Init(GetKits());

            var snapshot = _snapshotService.CreatePublishedSnapshot(previewToken: null);
            _snapshotAccessor.PublishedSnapshot = snapshot;

            Assert.IsNull(snapshot.Content.GetById(1).Parent);
            Assert.IsNull(snapshot.Content.GetById(2).Parent);
            Assert.IsNull(snapshot.Content.GetById(3).Parent);

            Assert.AreEqual(1, snapshot.Content.GetById(4).Parent?.Id);
            Assert.AreEqual(1, snapshot.Content.GetById(5).Parent?.Id);
            Assert.AreEqual(1, snapshot.Content.GetById(6).Parent?.Id);

            Assert.AreEqual(2, snapshot.Content.GetById(7).Parent?.Id);
            Assert.AreEqual(2, snapshot.Content.GetById(8).Parent?.Id);
            Assert.AreEqual(2, snapshot.Content.GetById(9).Parent?.Id);

            Assert.AreEqual(3, snapshot.Content.GetById(10).Parent?.Id);

            Assert.AreEqual(4, snapshot.Content.GetById(11).Parent?.Id);
            Assert.AreEqual(4, snapshot.Content.GetById(12).Parent?.Id);
        }

        [Test]
        public void MoveToRootTest()
        {
            Init(GetKits());

            // get snapshot
            var snapshot = _snapshotService.CreatePublishedSnapshot(previewToken: null);
            _snapshotAccessor.PublishedSnapshot = snapshot;

            // do some changes
            var kit = _source.Kits[10];
            _source.Kits[10] = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(kit.Node.Id, Guid.NewGuid(), 1, "-1,10", 4, -1, DateTime.Now, 0),
                DraftData = null,
                PublishedData = new ContentData
                {
                    Name = kit.PublishedData.Name,
                    Published = true,
                    TemplateId = 0,
                    VersionId = 1,
                    VersionDate = DateTime.Now,
                    WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]>(),
                    CultureInfos = new Dictionary<string, CultureVariation>()
                }
            };

            // notify
            _snapshotService.Notify(new[] { new ContentCacheRefresher.JsonPayload(10, TreeChangeTypes.RefreshBranch) }, out _, out _);

            // changes that *I* make are immediately visible on the current snapshot
            var documents = snapshot.Content.GetAtRoot().ToArray();
            AssertDocuments(documents, "N1", "N2", "N3", "N10");

            documents = snapshot.Content.GetById(3).Children().ToArray();
            AssertDocuments(documents);

            Assert.IsNull(snapshot.Content.GetById(10).Parent);
        }

        [Test]
        public void MoveFromRootTest()
        {
            Init(GetKits());

            // get snapshot
            var snapshot = _snapshotService.CreatePublishedSnapshot(previewToken: null);
            _snapshotAccessor.PublishedSnapshot = snapshot;

            // do some changes
            var kit = _source.Kits[1];
            _source.Kits[1] = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(kit.Node.Id, Guid.NewGuid(), 1, "-1,3,10,1", 1, 10, DateTime.Now, 0),
                DraftData = null,
                PublishedData = new ContentData
                {
                    Name = kit.PublishedData.Name,
                    Published = true,
                    TemplateId = 0,
                    VersionId = 1,
                    VersionDate = DateTime.Now,
                    WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]>(),
                    CultureInfos = new Dictionary<string, CultureVariation>()
                }
            };

            // notify
            _snapshotService.Notify(new[] { new ContentCacheRefresher.JsonPayload(1, TreeChangeTypes.RefreshBranch) }, out _, out _);

            // changes that *I* make are immediately visible on the current snapshot
            var documents = snapshot.Content.GetAtRoot().ToArray();
            AssertDocuments(documents, "N2", "N3");

            documents = snapshot.Content.GetById(10).Children().ToArray();
            AssertDocuments(documents, "N1");

            Assert.AreEqual(10, snapshot.Content.GetById(1).Parent?.Id);
        }

        [Test]
        public void ReOrderTest()
        {
            Init(GetKits());

            // get snapshot
            var snapshot = _snapshotService.CreatePublishedSnapshot(previewToken: null);
            _snapshotAccessor.PublishedSnapshot = snapshot;

            // do some changes
            var kit = _source.Kits[7];
            _source.Kits[7] = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(kit.Node.Id, Guid.NewGuid(), kit.Node.Level, kit.Node.Path, 1, kit.Node.ParentContentId, DateTime.Now, 0),
                DraftData = null,
                PublishedData = new ContentData
                {
                    Name = kit.PublishedData.Name,
                    Published = true,
                    TemplateId = 0,
                    VersionId = 1,
                    VersionDate = DateTime.Now,
                    WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]>(),
                    CultureInfos = new Dictionary<string, CultureVariation>()
                }
            };

            kit = _source.Kits[8];
            _source.Kits[8] = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(kit.Node.Id, Guid.NewGuid(), kit.Node.Level, kit.Node.Path, 3, kit.Node.ParentContentId, DateTime.Now, 0),
                DraftData = null,
                PublishedData = new ContentData
                {
                    Name = kit.PublishedData.Name,
                    Published = true,
                    TemplateId = 0,
                    VersionId = 1,
                    VersionDate = DateTime.Now,
                    WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]>(),
                    CultureInfos = new Dictionary<string, CultureVariation>()
                }
            };

            kit = _source.Kits[9];
            _source.Kits[9] = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(kit.Node.Id, Guid.NewGuid(), kit.Node.Level, kit.Node.Path, 2, kit.Node.ParentContentId, DateTime.Now, 0),
                DraftData = null,
                PublishedData = new ContentData
                {
                    Name = kit.PublishedData.Name,
                    Published = true,
                    TemplateId = 0,
                    VersionId = 1,
                    VersionDate = DateTime.Now,
                    WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]>(),
                    CultureInfos = new Dictionary<string, CultureVariation>()
                }
            };

            // notify
            _snapshotService.Notify(new[] { new ContentCacheRefresher.JsonPayload(kit.Node.ParentContentId, TreeChangeTypes.RefreshBranch) }, out _, out _);

            // changes that *I* make are immediately visible on the current snapshot
            var documents = snapshot.Content.GetById(kit.Node.ParentContentId).Children().ToArray();
            AssertDocuments(documents, "N7", "N9", "N8");
        }

        [Test]
        public void MoveTest()
        {
            Init(GetKits());

            // get snapshot
            var snapshot = _snapshotService.CreatePublishedSnapshot(previewToken: null);
            _snapshotAccessor.PublishedSnapshot = snapshot;

            // do some changes
            var kit = _source.Kits[4];
            _source.Kits[4] = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(kit.Node.Id, Guid.NewGuid(), kit.Node.Level, kit.Node.Path, 2, kit.Node.ParentContentId, DateTime.Now, 0),
                DraftData = null,
                PublishedData = new ContentData
                {
                    Name = kit.PublishedData.Name,
                    Published = true,
                    TemplateId = 0,
                    VersionId = 1,
                    VersionDate = DateTime.Now,
                    WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]>(),
                    CultureInfos = new Dictionary<string, CultureVariation>()
                }
            };

            kit = _source.Kits[5];
            _source.Kits[5] = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(kit.Node.Id, Guid.NewGuid(), kit.Node.Level, kit.Node.Path, 3, kit.Node.ParentContentId, DateTime.Now, 0),
                DraftData = null,
                PublishedData = new ContentData
                {
                    Name = kit.PublishedData.Name,
                    Published = true,
                    TemplateId = 0,
                    VersionId = 1,
                    VersionDate = DateTime.Now,
                    WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]>(),
                    CultureInfos = new Dictionary<string, CultureVariation>()
                }
            };

            kit = _source.Kits[6];
            _source.Kits[6] = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(kit.Node.Id, Guid.NewGuid(), kit.Node.Level, kit.Node.Path, 4, kit.Node.ParentContentId, DateTime.Now, 0),
                DraftData = null,
                PublishedData = new ContentData
                {
                    Name = kit.PublishedData.Name,
                    Published = true,
                    TemplateId = 0,
                    VersionId = 1,
                    VersionDate = DateTime.Now,
                    WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]>(),
                    CultureInfos = new Dictionary<string, CultureVariation>()
                }
            };

            kit = _source.Kits[7];
            _source.Kits[7] = new ContentNodeKit
            {
                ContentTypeId = 2,
                Node = new ContentNode(kit.Node.Id, Guid.NewGuid(), kit.Node.Level, "-1,1,7", 1, 1, DateTime.Now, 0),
                DraftData = null,
                PublishedData = new ContentData
                {
                    Name = kit.PublishedData.Name,
                    Published = true,
                    TemplateId = 0,
                    VersionId = 1,
                    VersionDate = DateTime.Now,
                    WriterId = 0,
                    Properties = new Dictionary<string, PropertyData[]>(),
                    CultureInfos = new Dictionary<string, CultureVariation>()
                }
            };

            // notify
            _snapshotService.Notify(new[]
            {
                // removal must come first
                new ContentCacheRefresher.JsonPayload(2, TreeChangeTypes.RefreshBranch),
                new ContentCacheRefresher.JsonPayload(1, TreeChangeTypes.RefreshBranch)
            }, out _, out _);

            // changes that *I* make are immediately visible on the current snapshot
            var documents = snapshot.Content.GetById(1).Children().ToArray();
            AssertDocuments(documents, "N7", "N4", "N5", "N6");

            documents = snapshot.Content.GetById(2).Children().ToArray();
            AssertDocuments(documents, "N9", "N8");

            Assert.AreEqual(1, snapshot.Content.GetById(7).Parent?.Id);
        }

        private void AssertDocuments(IPublishedContent[] documents, params string[] names)
        {
            Assert.AreEqual(names.Length, documents.Length);
            for (var i = 0; i < names.Length; i++)
                Assert.AreEqual(names[i], documents[i].Name());
        }
    }
}
