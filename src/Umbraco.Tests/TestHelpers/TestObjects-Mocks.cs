using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;
using LightInject;
using Moq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

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
        public static IDatabaseFactory GetDatabaseFactoryMock(bool configured = true, bool canConnect = true)
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
        public static ServiceContext GetServiceContextMock(IServiceFactory container = null)
        {
            return new ServiceContext(
                MockService<IContentService>(),
                MockService<IMediaService>(),
                MockService<IContentTypeService>(),
                MockService<IMediaTypeService>(),
                MockService<IDataTypeService>(),
                MockService<IFileService>(),
                MockService<ILocalizationService>(),
                MockService<IPackagingService>(),
                MockService<IEntityService>(),
                MockService<IRelationService>(),
                MockService<IMemberGroupService>(),
                MockService<IMemberTypeService>(),
                MockService<IMemberService>(),
                MockService<IUserService>(),
                MockService<ISectionService>(),
                MockService<IApplicationTreeService>(),
                MockService<ITagService>(),
                MockService<INotificationService>(),
                MockService<ILocalizedTextService>(),
                MockService<IAuditService>(),
                MockService<IDomainService>(),
                MockService<ITaskService>(),
                MockService<IMacroService>());
        }

        private static T MockService<T>(IServiceFactory container = null)
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
        public static DbConnection GetDbConnection()
        {
            return new MockDbConnection();
        }

        /// <summary>
        /// Gets an Umbraco context.
        /// </summary>
        /// <returns>An Umbraco context.</returns>
        /// <remarks>This should be the minimum Umbraco context.</remarks>
        public static UmbracoContext GetUmbracoContextMock(IUmbracoContextAccessor accessor = null)
        {
            var httpContext = Mock.Of<HttpContextBase>();

            //var facadeService = Mock.Of<IFacadeService>();
            var facadeMock = new Mock<IFacade>();
            facadeMock.Setup(x => x.MemberCache).Returns(Mock.Of<IPublishedMemberCache>());
            var facade = facadeMock.Object;
            var facadeServiceMock = new Mock<IFacadeService>();
            facadeServiceMock.Setup(x => x.CreateFacade(It.IsAny<string>())).Returns(facade);
            var facadeService = facadeServiceMock.Object;

            var webSecurity = new Mock<WebSecurity>(null, null).Object;
            var settings = GetUmbracoSettings();
            var urlProviders = Enumerable.Empty<IUrlProvider>();

            if (accessor == null) accessor = new TestUmbracoContextAccessor();
            return UmbracoContext.EnsureContext(accessor, httpContext, facadeService, webSecurity, settings, urlProviders, true);
        }

        public static IUmbracoSettingsSection GetUmbracoSettings()
        {
            var umbracoSettingsMock = new Mock<IUmbracoSettingsSection>();
            var webRoutingSectionMock = new Mock<IWebRoutingSection>();
            webRoutingSectionMock.Setup(x => x.UrlProviderMode).Returns(UrlProviderMode.Auto.ToString());
            umbracoSettingsMock.Setup(x => x.WebRouting).Returns(webRoutingSectionMock.Object);
            return umbracoSettingsMock.Object;
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