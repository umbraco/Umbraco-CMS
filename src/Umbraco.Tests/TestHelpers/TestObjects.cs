using System;
using Moq;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Persistance.SqlCe;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides objects for tests.
    /// </summary>
    internal partial class TestObjects
    {
        private readonly IRegister _register;

        public TestObjects(IRegister register)
        {
            _register = register;
        }

        /// <summary>
        /// Gets an UmbracoDatabase.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <returns>An UmbracoDatabase.</returns>
        /// <remarks>This is just a void database that has no actual database but pretends to have an open connection
        /// that can begin a transaction.</remarks>
        public UmbracoDatabase GetUmbracoSqlCeDatabase(ILogger logger)
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
        public UmbracoDatabase GetUmbracoSqlServerDatabase(ILogger logger)
        {
            var syntax = new SqlServerSyntaxProvider(); // do NOT try to get the server's version!
            var connection = GetDbConnection();
            var sqlContext = new SqlContext(syntax, DatabaseType.SqlServer2008, Mock.Of<IPocoDataFactory>());
            return new UmbracoDatabase(connection, sqlContext, logger, TestHelper.BulkSqlInsertProvider);
        }

        private Lazy<T> GetLazyService<T>(IFactory container, Func<IFactory, T> ctor)
            where T : class
        {
            return new Lazy<T>(() => container?.TryGetInstance<T>() ?? ctor(container));
        }

        private T GetRepo<T>(IFactory container)
            where T : class, IRepository
        {
            return container?.TryGetInstance<T>() ?? Mock.Of<T>();
        }

        public IScopeProvider GetScopeProvider(ILogger logger, ITypeFinder typeFinder = null, FileSystems fileSystems = null, IUmbracoDatabaseFactory databaseFactory = null)
        {
            if (databaseFactory == null)
            {
                // var mappersBuilder = new MapperCollectionBuilder(Current.Container); // FIXME:
                // mappersBuilder.AddCore();
                // var mappers = mappersBuilder.CreateCollection();
                var mappers = Current.Factory.GetInstance<IMapperCollection>();
                databaseFactory = new UmbracoDatabaseFactory(logger,
                    SettingsForTests.DefaultGlobalSettings,
                    new ConnectionStrings(),
                    Constants.System.UmbracoConnectionName,
                    new Lazy<IMapperCollection>(() => mappers),
                    TestHelper.DbProviderFactoryCreator);
            }

            typeFinder ??= new TypeFinder(logger, new DefaultUmbracoAssemblyProvider(GetType().Assembly), new VaryingRuntimeHash());
            fileSystems ??= new FileSystems(Current.Factory, logger, TestHelper.IOHelper, SettingsForTests.GenerateMockGlobalSettings(), TestHelper.GetHostingEnvironment());
            var coreDebug = TestHelper.CoreDebugSettings;
            var mediaFileSystem = Mock.Of<IMediaFileSystem>();
            var scopeProvider = new ScopeProvider(databaseFactory, fileSystems, coreDebug, mediaFileSystem, logger, typeFinder, NoAppCache.Instance);
            return scopeProvider;
        }

    }
}
