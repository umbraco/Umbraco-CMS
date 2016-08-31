using LightInject;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
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
        protected IMapperCollection Mappers { get; private set; }

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
            container.ConfigureUmbracoCore();

            container.RegisterSingleton<ILogger>(factory => Mock.Of<ILogger>());
            container.RegisterSingleton<IProfiler>(factory => Mock.Of<IProfiler>());

            var logger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());
            var pluginManager = PluginManager.Current = new PluginManager(new NullCacheProvider(),
                logger,
                false);
            container.RegisterInstance(pluginManager);

            MapperCollectionBuilder.Register(container)
                .AddProducer(() => PluginManager.Current.ResolveAssignedMapperTypes());
            Mappers = container.GetInstance<IMapperCollection>();
            
            var mappers = new NPoco.MapperCollection { new PocoMapper() };
            var pocoDataFactory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, mappers).Init());
            SqlContext = new SqlContext(sqlSyntax, pocoDataFactory, DatabaseType.SQLCe);

            SetUp();
        }

        public virtual void SetUp()
        {}

        [TearDown]
        public virtual void TearDown()
        {
            //MappingResolver.Reset();
            PluginManager.Current = null;
            Current.Reset();
        }
    }
}