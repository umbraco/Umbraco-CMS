using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Umbraco.Tests.Components;

namespace Umbraco.Tests.TestHelpers
{
    [TestFixture]
    public abstract class BaseUsingSqlCeSyntax
    {
        protected IMapperCollection Mappers { get; private set; }

        protected ISqlContext SqlContext { get; private set; }

        internal TestObjects TestObjects = new TestObjects(null);

        protected Sql<ISqlContext> Sql()
        {
            return NPoco.Sql.BuilderFor(SqlContext);
        }

        [SetUp]
        public virtual void Initialize()
        {
            Current.Reset();

            var sqlSyntax = new SqlCeSyntaxProvider();

            var container = RegisterFactory.Create();

            var logger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());
            var typeLoader = new TypeLoader(NullCacheProvider.Instance,
                LocalTempStorage.Default,
                logger,
                false);

            var composition = new Composition(container, typeLoader, Mock.Of<IProfilingLogger>(), ComponentTests.MockRuntimeState(RuntimeLevel.Run));

            composition.RegisterUnique<ILogger>(_ => Mock.Of<ILogger>());
            composition.RegisterUnique<IProfiler>(_ => Mock.Of<IProfiler>());

            composition.RegisterUnique(typeLoader);

            composition.WithCollectionBuilder<MapperCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetAssignedMapperTypes());

            var factory = Current.Factory = composition.CreateFactory();

            Mappers = factory.GetInstance<IMapperCollection>();

            var pocoMappers = new NPoco.MapperCollection { new PocoMapper() };
            var pocoDataFactory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, pocoMappers).Init());
            SqlContext = new SqlContext(sqlSyntax, DatabaseType.SQLCe, pocoDataFactory, Mappers);

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
