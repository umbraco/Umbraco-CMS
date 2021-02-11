using System;
using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Persistence.SqlCe;
using Umbraco.Web.Composing;

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
