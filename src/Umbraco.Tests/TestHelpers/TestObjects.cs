using System;
using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Persistence.SqlCe;
using Umbraco.Web.Composing;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides objects for tests.
    /// </summary>
    internal partial class TestObjects
    {

        public TestObjects()
        {
        }

        /// <summary>
        /// Gets an UmbracoDatabase.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <returns>An UmbracoDatabase.</returns>
        /// <remarks>This is just a void database that has no actual database but pretends to have an open connection
        /// that can begin a transaction.</remarks>
        public UmbracoDatabase GetUmbracoSqlCeDatabase(ILogger<UmbracoDatabase> logger)
        {
            var syntax = new SqlCeSyntaxProvider();
            var connection = GetDbConnection();
            var sqlContext = new SqlContext(syntax, DatabaseType.SQLCe, Mock.Of<IPocoDataFactory>());
            return new UmbracoDatabase(connection, sqlContext, logger, TestHelper.BulkSqlInsertProvider);
        }

        /// <summary>
        /// Gets an UmbracoDatabase.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <returns>An UmbracoDatabase.</returns>
        /// <remarks>This is just a void database that has no actual database but pretends to have an open connection
        /// that can begin a transaction.</remarks>
        public UmbracoDatabase GetUmbracoSqlServerDatabase(ILogger<UmbracoDatabase> logger)
        {
            var syntax = new SqlServerSyntaxProvider(); // do NOT try to get the server's version!
            var connection = GetDbConnection();
            var sqlContext = new SqlContext(syntax, DatabaseType.SqlServer2008, Mock.Of<IPocoDataFactory>());
            return new UmbracoDatabase(connection, sqlContext, logger, TestHelper.BulkSqlInsertProvider);
        }

        public IScopeProvider GetScopeProvider(ILoggerFactory loggerFactory, FileSystems fileSystems = null, IUmbracoDatabaseFactory databaseFactory = null)
        {
            var globalSettings = new GlobalSettings();
            var connectionString = ConfigurationManager.ConnectionStrings[Constants.System.UmbracoConnectionName].ConnectionString;
            var connectionStrings = new ConnectionStrings { UmbracoConnectionString = new ConfigConnectionString(Constants.System.UmbracoConnectionName, connectionString) };
            var coreDebugSettings = new CoreDebugSettings();

            if (databaseFactory == null)
            {
                // var mappersBuilder = new MapperCollectionBuilder(Current.Container); // FIXME:
                // mappersBuilder.AddCore();
                // var mappers = mappersBuilder.CreateCollection();
                var mappers = Current.Factory.GetRequiredService<IMapperCollection>();
                databaseFactory = new UmbracoDatabaseFactory(
                    loggerFactory.CreateLogger<UmbracoDatabaseFactory>(),
                    loggerFactory,
                    globalSettings,
                    connectionStrings,
                    new Lazy<IMapperCollection>(() => mappers),
                    TestHelper.DbProviderFactoryCreator,
                    new DatabaseSchemaCreatorFactory(Mock.Of<ILogger<DatabaseSchemaCreator>>(),loggerFactory, new UmbracoVersion()));
            }

            fileSystems ??= new FileSystems(Current.Factory, loggerFactory.CreateLogger<FileSystems>(), loggerFactory, TestHelper.IOHelper, Options.Create(globalSettings), TestHelper.GetHostingEnvironment());
            var coreDebug = TestHelper.CoreDebugSettings;
            var mediaFileSystem = Mock.Of<IMediaFileSystem>();
            return new ScopeProvider(databaseFactory, fileSystems, Options.Create(coreDebugSettings), mediaFileSystem, loggerFactory.CreateLogger<ScopeProvider>(), loggerFactory, NoAppCache.Instance);
        }

    }
}
