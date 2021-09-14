using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache
{
    [TestFixture]
    public class PublishedSnapshotServiceTestBase
    {
        protected virtual IPublishedModelFactory PublishedModelFactory { get; } = new NoopPublishedModelFactory();
        protected IPublishedValueFallback PublishedValueFallback { get; private set; }
        protected IPublishedSnapshotService SnapshotService { get; private set; }
        protected IVariationContextAccessor VariationContextAccessor { get; private set; }
        protected TestPublishedSnapshotAccessor PublishedSnapshotAccessor { get; private set; }
        protected TestNuCacheContentService NuCacheContentService { get; private set; }
        protected virtual PropertyValueConverterCollection PropertyValueConverterCollection
            => new PropertyValueConverterCollection(() => new[]
                {
                    new TestSimpleTinyMceValueConverter()
                });

        [SetUp]
        public virtual void Setup()
        {
            VariationContextAccessor = new TestVariationContextAccessor();
            PublishedSnapshotAccessor = new TestPublishedSnapshotAccessor();
        }

        [TearDown]
        public void Teardown()
        {
            SnapshotService?.Dispose();
        }

        /// <summary>
        /// Used as a property editor for any test property that has an editor alias called "Umbraco.Void.RTE"
        /// </summary>
        private class TestSimpleTinyMceValueConverter : SimpleTinyMceValueConverter
        {
            public override bool IsConverter(IPublishedPropertyType propertyType)
                => propertyType.EditorAlias == "Umbraco.Void.RTE";
        }

        private static DataType[] GetDefaultDataTypes()
        {
            var serializer = new ConfigurationEditorJsonSerializer();

            // create data types, property types and content types
            var dataType = new DataType(new VoidEditor("Editor", Mock.Of<IDataValueEditorFactory>()), serializer) { Id = 3 };

            return new[] { dataType };
        }

        private static ServiceContext GetServiceContext(ContentType[] contentTypes, DataType[] dataTypes, out IDataTypeService dataTypeService)
        {
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
        protected IPublishedSnapshot GetPublishedSnapshot()
        {
            var snapshot = SnapshotService.CreatePublishedSnapshot(previewToken: null);
            PublishedSnapshotAccessor.SetCurrent(snapshot);
            return snapshot;
        }

        /// <summary>
        /// Initializes the <see cref="IPublishedSnapshotService'"/> with a source of data
        /// </summary>
        /// <param name="contentNodeKits"></param>
        /// <param name="contentTypes"></param>
        protected void Init(IEnumerable<ContentNodeKit> contentNodeKits, ContentType[] contentTypes, DataType[] dataTypes = null)
        {
            // create a data source for NuCache
            NuCacheContentService = new TestNuCacheContentService(contentNodeKits);

            var runtime = Mock.Of<IRuntimeState>();
            Mock.Get(runtime).Setup(x => x.Level).Returns(RuntimeLevel.Run);

            // create a service context
            ServiceContext serviceContext = GetServiceContext(
                contentTypes,
                dataTypes ?? GetDefaultDataTypes(),
                out IDataTypeService dataTypeService);

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
                PublishedModelFactory,
                PropertyValueConverterCollection,
                dataTypeService);            

            var typeFinder = TestHelper.GetTypeFinder();

            var globalSettings = new GlobalSettings();
            var nuCacheSettings = new NuCacheSettings();
            
            // at last, create the complete NuCache snapshot service!
            var options = new PublishedSnapshotServiceOptions { IgnoreLocalDb = true };
            SnapshotService = new PublishedSnapshotService(
                options,
                Mock.Of<ISyncBootStateAccessor>(x => x.GetSyncBootState() == SyncBootState.WarmBoot),
                new SimpleMainDom(),
                serviceContext,
                contentTypeFactory,
                PublishedSnapshotAccessor,
                VariationContextAccessor,
                Mock.Of<IProfilingLogger>(),
                NullLoggerFactory.Instance,
                scopeProvider,
                NuCacheContentService,
                new TestDefaultCultureAccessor(),
                Microsoft.Extensions.Options.Options.Create(globalSettings),
                PublishedModelFactory,
                TestHelper.GetHostingEnvironment(),
                Microsoft.Extensions.Options.Options.Create(nuCacheSettings),
                //ContentNestedDataSerializerFactory,
                new ContentDataSerializer(new DictionaryOfPropertyDataSerializer()));

            // invariant is the current default
            VariationContextAccessor.VariationContext = new VariationContext();

            PublishedValueFallback = new PublishedValueFallback(serviceContext, VariationContextAccessor);
        }
    }
}
