using System;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.TestHelpers
{
    [TestFixture]
    public abstract class BaseUsingSqlSyntax
    {
        protected IMapperCollection Mappers { get; private set; }

        protected ISqlContext SqlContext { get; private set; }

        protected Sql<ISqlContext> Sql()
        {
            return NPoco.Sql.BuilderFor(SqlContext);
        }

        [SetUp]
        public virtual void Setup()
        {
            var container = TestHelper.GetServiceCollection();
            var typeLoader = TestHelper.GetMockedTypeLoader();

            var composition = new Composition(container, typeLoader, Mock.Of<IProfilingLogger>(), Mock.Of<IRuntimeState>(), TestHelper.IOHelper, AppCaches.NoCache);

            composition.WithCollectionBuilder<MapperCollectionBuilder>()
                .AddCoreMappers();

            composition.RegisterUnique<ISqlContext>(_ => SqlContext);

            var factory = composition.CreateFactory();
            var pocoMappers = new NPoco.MapperCollection { new PocoMapper() };
            var pocoDataFactory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, pocoMappers).Init());
            var sqlSyntax = new SqlServerSyntaxProvider();
            SqlContext = new SqlContext(sqlSyntax, DatabaseType.SqlServer2012, pocoDataFactory, new Lazy<IMapperCollection>(() => factory.GetInstance<IMapperCollection>()));
            Mappers = factory.GetInstance<IMapperCollection>();
        }
    }
}
