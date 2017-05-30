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
using Umbraco.Core.Composing;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.TestHelpers
{
    [TestFixture]
    public abstract class BaseUsingSqlCeSyntax
    {
        protected IMapperCollection Mappers { get; private set; }

        protected SqlContext SqlContext { get; private set; }

        internal TestObjects TestObjects = new TestObjects(null);

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
            var pluginManager = new TypeLoader(new NullCacheProvider(),
                logger,
                false);
            container.RegisterInstance(pluginManager);

            container.RegisterCollectionBuilder<MapperCollectionBuilder>()
                .Add(() => Current.TypeLoader.GetAssignedMapperTypes());
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
            Current.Reset();
        }
    }
}