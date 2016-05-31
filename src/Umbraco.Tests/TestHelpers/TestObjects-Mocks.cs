using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Examine;
using Moq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using UmbracoExamine;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides objects for tests.
    /// </summary>
    internal static partial class TestObjects
    {
        /// <summary>
        /// Gets a mocked IDatabaseFactory.
        /// </summary>
        /// <returns>An IDatabaseFactory.</returns>
        /// <param name="configured">A value indicating whether the factory is configured.</param>
        /// <param name="canConnect">A value indicating whether the factory can connect to the database.</param>
        /// <remarks>This is just a void factory that has no actual database.</remarks>
        public static IDatabaseFactory GetIDatabaseFactoryMock(bool configured = true, bool canConnect = true)
        {
            var databaseFactoryMock = new Mock<IDatabaseFactory>();
            databaseFactoryMock.Setup(x => x.Configured).Returns(configured);
            databaseFactoryMock.Setup(x => x.CanConnect).Returns(canConnect);

            // can get a database - but don't try to use it!
            if (configured && canConnect)
                databaseFactoryMock.Setup(x => x.GetDatabase()).Returns(TestObjects.GetUmbracoSqlCeDatabase(Mock.Of<ILogger>()));

            return databaseFactoryMock.Object;
        }

        /// <summary>
        /// Gets a mocked service context built with mocked services.
        /// </summary>
        /// <returns>A ServiceContext.</returns>
        public static ServiceContext GetServiceContextMock()
        {
            return new ServiceContext(
                new Mock<IContentService>().Object,
                new Mock<IMediaService>().Object,
                new Mock<IContentTypeService>().Object,
                new Mock<IMediaTypeService>().Object,
                new Mock<IDataTypeService>().Object,
                new Mock<IFileService>().Object,
                new Mock<ILocalizationService>().Object,
                new Mock<IPackagingService>().Object,
                new Mock<IEntityService>().Object,
                new Mock<IRelationService>().Object,
                new Mock<IMemberGroupService>().Object,
                new Mock<IMemberTypeService>().Object,
                new Mock<IMemberService>().Object,
                new Mock<IUserService>().Object,
                new Mock<ISectionService>().Object,
                new Mock<IApplicationTreeService>().Object,
                new Mock<ITagService>().Object,
                new Mock<INotificationService>().Object,
                new Mock<ILocalizedTextService>().Object,
                new Mock<IAuditService>().Object,
                new Mock<IDomainService>().Object,
                new Mock<ITaskService>().Object,
                new Mock<IMacroService>().Object);
        }

        /// <summary>
        /// Gets an opened database connection that can begin a transaction.
        /// </summary>
        /// <returns>A DbConnection.</returns>
        /// <remarks>This is because NPoco wants a DbConnection, NOT an IDbConnection,
        /// and DbConnection is hard to mock so we create our own class here.</remarks>
        public static DbConnection GetDbConnection()
        {
            return new MockDbConnection();
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
        
        #endregion
    }
}