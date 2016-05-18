using LightInject;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Persistence;
using Umbraco.Core.Plugins;

namespace Umbraco.Tests.TestHelpers
{
    [TestFixture]
    public abstract class BaseUsingSqlCeSyntax
    {
        private MappingResolver _mappingResolver;

        protected IMappingResolver MappingResolver => _mappingResolver;

        protected SqlContext SqlContext { get; private set; }

        protected Sql<SqlContext> Sql()
        {
            return NPoco.Sql.BuilderFor(SqlContext);
        }

        [SetUp]
        public virtual void Initialize()
        {
            var sqlSyntax = new SqlCeSyntaxProvider();

            var container = new ServiceContainer();
            container.EnableAnnotatedConstructorInjection();

            container.RegisterSingleton<ILogger>(factory => Mock.Of<ILogger>());
            container.RegisterSingleton<IProfiler>(factory => Mock.Of<IProfiler>());

            _mappingResolver = new MappingResolver(container, Mock.Of<ILogger>(),
                () => PluginManager.Current.ResolveAssignedMapperTypes());

            var logger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());
            
            PluginManager.Current = new PluginManager(new ActivatorServiceProvider(), new NullCacheProvider(), 
                logger,
                false);

            var mappers = new MapperCollection { new PocoMapper() };
            var pocoDataFactory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, mappers).Init());
            SqlContext = new SqlContext(sqlSyntax, pocoDataFactory, DatabaseType.SQLCe);

            Resolution.Freeze();
            SetUp();
        }

        public virtual void SetUp()
        {}

        [TearDown]
        public virtual void TearDown()
        {
            Resolution.Reset();
            //MappingResolver.Reset();
            PluginManager.Current = null;
        }
    }
}