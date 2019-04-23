using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides objects for tests.
    /// </summary>
    public partial class TestObjects
    {
        /// <summary>
        /// Gets a mocked IUmbracoDatabaseFactory.
        /// </summary>
        /// <returns>An IUmbracoDatabaseFactory.</returns>
        /// <param name="configured">A value indicating whether the factory is configured.</param>
        /// <param name="canConnect">A value indicating whether the factory can connect to the database.</param>
        /// <remarks>This is just a void factory that has no actual database.</remarks>
        public IUmbracoDatabaseFactory GetDatabaseFactoryMock(bool configured = true, bool canConnect = true)
        {
            var sqlSyntax = new SqlCeSyntaxProvider();
            var sqlContext = Mock.Of<ISqlContext>();
            Mock.Get(sqlContext).Setup(x => x.SqlSyntax).Returns(sqlSyntax);

            var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
            databaseFactoryMock.Setup(x => x.Configured).Returns(configured);
            databaseFactoryMock.Setup(x => x.CanConnect).Returns(canConnect);
            databaseFactoryMock.Setup(x => x.SqlContext).Returns(sqlContext);

            // can create a database - but don't try to use it!
            if (configured && canConnect)
                databaseFactoryMock.Setup(x => x.CreateDatabase()).Returns(GetUmbracoSqlCeDatabase(Mock.Of<ILogger>()));

            return databaseFactoryMock.Object;
        }

        /// <summary>
        /// Gets a mocked service context built with mocked services.
        /// </summary>
        /// <returns>A ServiceContext.</returns>
        public ServiceContext GetServiceContextMock(IFactory container = null)
        {
            // FIXME: else some tests break - figure it out
            container = null;

            return ServiceContext.CreatePartial(
                MockService<IContentService>(container),
                MockService<IMediaService>(container),
                MockService<IContentTypeService>(container),
                MockService<IMediaTypeService>(container),
                MockService<IDataTypeService>(container),
                MockService<IFileService>(container),
                MockService<ILocalizationService>(container),
                MockService<IPackagingService>(container),
                MockService<IEntityService>(container),
                MockService<IRelationService>(container),
                MockService<IMemberGroupService>(container),
                MockService<IMemberTypeService>(container),
                MockService<IMemberService>(container),
                MockService<IUserService>(container),
                MockService<ITagService>(container),
                MockService<INotificationService>(container),
                MockService<ILocalizedTextService>(container),
                MockService<IAuditService>(container),
                MockService<IDomainService>(container),
                MockService<IMacroService>(container));
        }

        private T MockService<T>(IFactory container = null)
            where T : class
        {
            return container?.TryGetInstance<T>() ?? new Mock<T>().Object;
        }

        /// <summary>
        /// Gets an opened database connection that can begin a transaction.
        /// </summary>
        /// <returns>A DbConnection.</returns>
        /// <remarks>This is because NPoco wants a DbConnection, NOT an IDbConnection,
        /// and DbConnection is hard to mock so we create our own class here.</remarks>
        public DbConnection GetDbConnection()
        {
            return new MockDbConnection();
        }

        /// <summary>
        /// Gets an Umbraco context.
        /// </summary>
        /// <returns>An Umbraco context.</returns>
        /// <remarks>This should be the minimum Umbraco context.</remarks>
        public UmbracoContext GetUmbracoContextMock(IUmbracoContextAccessor accessor = null)
        {
            var httpContext = Mock.Of<HttpContextBase>();

            var publishedSnapshotMock = new Mock<IPublishedSnapshot>();
            publishedSnapshotMock.Setup(x => x.Members).Returns(Mock.Of<IPublishedMemberCache>());
            var publishedSnapshot = publishedSnapshotMock.Object;
            var publishedSnapshotServiceMock = new Mock<IPublishedSnapshotService>();
            publishedSnapshotServiceMock.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(publishedSnapshot);
            var publishedSnapshotService = publishedSnapshotServiceMock.Object;

            var umbracoSettings = GetUmbracoSettings();
            var globalSettings = GetGlobalSettings();
            var urlProviders = new UrlProviderCollection(Enumerable.Empty<IUrlProvider>());
            var mediaUrlProviders = new MediaUrlProviderCollection(Enumerable.Empty<IMediaUrlProvider>());

            if (accessor == null) accessor = new TestUmbracoContextAccessor();

            var umbracoContextFactory = new UmbracoContextFactory(
                accessor,
                publishedSnapshotService,
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                umbracoSettings,
                globalSettings,
                urlProviders,
                mediaUrlProviders,
                Mock.Of<IUserService>());

            return umbracoContextFactory.EnsureUmbracoContext(httpContext).UmbracoContext;
        }

        public IUmbracoSettingsSection GetUmbracoSettings()
        {
            // FIXME: Why not use the SettingsForTest.GenerateMock ... ?
            // FIXME: Shouldn't we use the default ones so they are the same instance for each test?

            var umbracoSettingsMock = new Mock<IUmbracoSettingsSection>();
            var webRoutingSectionMock = new Mock<IWebRoutingSection>();
            webRoutingSectionMock.Setup(x => x.UrlProviderMode).Returns(UrlProviderMode.Auto.ToString());
            umbracoSettingsMock.Setup(x => x.WebRouting).Returns(webRoutingSectionMock.Object);
            return umbracoSettingsMock.Object;
        }

        public IGlobalSettings GetGlobalSettings()
        {
            return SettingsForTests.GetDefaultGlobalSettings();
        }

        public IFileSystems GetFileSystemsMock()
        {
            var fileSystems = Mock.Of<IFileSystems>();

            MockFs(fileSystems, x => x.MacroPartialsFileSystem);
            MockFs(fileSystems, x => x.MvcViewsFileSystem);
            MockFs(fileSystems, x => x.PartialViewsFileSystem);
            MockFs(fileSystems, x => x.ScriptsFileSystem);
            MockFs(fileSystems, x => x.StylesheetsFileSystem);

            return fileSystems;
        }

        private void MockFs(IFileSystems fileSystems, Expression<Func<IFileSystems, IFileSystem>> fileSystem)
        {
            var fs = Mock.Of<IFileSystem>();
            Mock.Get(fileSystems).Setup(fileSystem).Returns(fs);
        }

        #region Inner classes

        private class MockDbConnection : DbConnection
        {
            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            {
                return Mock.Of<DbTransaction>(); // enough here
            }

            public override void Close()
            {
                throw new NotImplementedException();
            }

            public override void ChangeDatabase(string databaseName)
            {
                throw new NotImplementedException();
            }

            public override void Open()
            {
                throw new NotImplementedException();
            }

            public override string ConnectionString { get; set; }

            protected override DbCommand CreateDbCommand()
            {
                throw new NotImplementedException();
            }

            public override string Database { get; }
            public override string DataSource { get; }
            public override string ServerVersion { get; }
            public override ConnectionState State => ConnectionState.Open; // else NPoco reopens
        }

        public class TestDataTypeService : IDataTypeService
        {
            public TestDataTypeService()
            {
                DataTypes = new Dictionary<int, IDataType>();
            }

            public TestDataTypeService(params IDataType[] dataTypes)
            {
                DataTypes = dataTypes.ToDictionary(x => x.Id, x => x);
            }

            public TestDataTypeService(IEnumerable<IDataType> dataTypes)
            {
                DataTypes = dataTypes.ToDictionary(x => x.Id, x => x);
            }

            public Dictionary<int, IDataType> DataTypes { get; }

            public Attempt<OperationResult<OperationResultType, EntityContainer>> CreateContainer(int parentId, string name, int userId = -1)
            {
                throw new NotImplementedException();
            }

            public Attempt<OperationResult> SaveContainer(EntityContainer container, int userId = -1)
            {
                throw new NotImplementedException();
            }

            public EntityContainer GetContainer(int containerId)
            {
                throw new NotImplementedException();
            }

            public EntityContainer GetContainer(Guid containerId)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<EntityContainer> GetContainers(string folderName, int level)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<EntityContainer> GetContainers(IDataType dataType)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
            {
                throw new NotImplementedException();
            }

            public Attempt<OperationResult> DeleteContainer(int containerId, int userId = -1)
            {
                throw new NotImplementedException();
            }

            public Attempt<OperationResult<OperationResultType, EntityContainer>> RenameContainer(int id, string name, int userId = -1)
            {
                throw new NotImplementedException();
            }

            public IDataType GetDataType(string name)
            {
                throw new NotImplementedException();
            }

            public IDataType GetDataType(int id)
            {
                DataTypes.TryGetValue(id, out var dataType);
                return dataType;
            }

            public IDataType GetDataType(Guid id)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IDataType> GetAll(params int[] ids)
            {
                if (ids.Length == 0) return DataTypes.Values;
                return ids.Select(x => DataTypes.TryGetValue(x, out var dataType) ? dataType : null).WhereNotNull();
            }

            public void Save(IDataType dataType, int userId = -1)
            {
                throw new NotImplementedException();
            }

            public void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId = -1)
            {
                throw new NotImplementedException();
            }

            public void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId, bool raiseEvents)
            {
                throw new NotImplementedException();
            }

            public void Delete(IDataType dataType, int userId = -1)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IDataType> GetByEditorAlias(string propertyEditorAlias)
            {
                throw new NotImplementedException();
            }

            public Attempt<OperationResult<MoveOperationStatusType>> Move(IDataType toMove, int parentId)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
