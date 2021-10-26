using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.SqlCe;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Extensions;
using Umbraco.Web;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides objects for tests.
    /// </summary>
    internal partial class TestObjects
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
            var sqlSyntax = new SqlCeSyntaxProvider(Options.Create(new GlobalSettings()));
            var sqlContext = Mock.Of<ISqlContext>();
            Mock.Get(sqlContext).Setup(x => x.SqlSyntax).Returns(sqlSyntax);

            var databaseFactoryMock = new Mock<IUmbracoDatabaseFactory>();
            databaseFactoryMock.Setup(x => x.Configured).Returns(configured);
            databaseFactoryMock.Setup(x => x.CanConnect).Returns(canConnect);
            databaseFactoryMock.Setup(x => x.SqlContext).Returns(sqlContext);

            // can create a database - but don't try to use it!
            if (configured && canConnect)
                databaseFactoryMock.Setup(x => x.CreateDatabase()).Returns(GetUmbracoSqlCeDatabase(Mock.Of<ILogger<UmbracoDatabase>>()));

            return databaseFactoryMock.Object;
        }

        /// <summary>
        /// Gets a mocked service context built with mocked services.
        /// </summary>
        /// <returns>A ServiceContext.</returns>
        public ServiceContext GetServiceContextMock(IServiceProvider container = null)
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

        private T MockService<T>(IServiceProvider container = null)
            where T : class
        {
            return container?.GetService<T>() ?? new Mock<T>().Object;
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
        public IUmbracoContext GetUmbracoContextMock(IUmbracoContextAccessor accessor = null)
        {

            var publishedSnapshotMock = new Mock<IPublishedSnapshot>();
            publishedSnapshotMock.Setup(x => x.Members).Returns(Mock.Of<IPublishedMemberCache>());
            var publishedSnapshot = publishedSnapshotMock.Object;
            var publishedSnapshotServiceMock = new Mock<IPublishedSnapshotService>();
            publishedSnapshotServiceMock.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(publishedSnapshot);
            var publishedSnapshotService = publishedSnapshotServiceMock.Object;

            var globalSettings = GetGlobalSettings();

            if (accessor == null) accessor = new TestUmbracoContextAccessor();

            var httpContextAccessor = TestHelper.GetHttpContextAccessor();

            var umbracoContextFactory = new UmbracoContextFactory(
                accessor,
                publishedSnapshotService,
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                globalSettings,
                Mock.Of<IUserService>(),
                TestHelper.GetHostingEnvironment(),
                TestHelper.UriUtility,
                httpContextAccessor,
                new AspNetCookieManager(httpContextAccessor));

            return umbracoContextFactory.EnsureUmbracoContext().UmbracoContext;
        }

        public GlobalSettings GetGlobalSettings()
        {
            return new GlobalSettings();
        }
        public FileSystems GetFileSystemsMock()
        {
            var fileSystems = FileSystemsCreator.CreateTestFileSystems(
                NullLoggerFactory.Instance,
                Mock.Of<IIOHelper>(),
                Mock.Of<IOptions<GlobalSettings>>(),
                Mock.Of<Cms.Core.Hosting.IHostingEnvironment>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<IFileSystem>()
            );

            return fileSystems;
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

            public IReadOnlyDictionary<Udi, IEnumerable<string>> GetReferences(int id)
            {
                throw new NotImplementedException();
            }
        }

        #endregion


    }
}
