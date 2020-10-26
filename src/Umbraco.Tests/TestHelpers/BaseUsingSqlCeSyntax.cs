using System;
using System.IO;
using Moq;
using NPoco;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence;
using Umbraco.Infrastructure.Composing;
using Umbraco.Persistance.SqlCe;
using Umbraco.Web;
using Current = Umbraco.Web.Composing.Current;

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

            var wrapper = (ServiceCollectionRegistryAdapter) TestHelper.GetRegister();

            var ioHelper = TestHelper.IOHelper;
            var logger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());
            var typeFinder = TestHelper.GetTypeFinder();
            var typeLoader = new TypeLoader(typeFinder, NoAppCache.Instance,
                new DirectoryInfo(ioHelper.MapPath("~/App_Data/TEMP")),
                Mock.Of<ILogger<TypeLoader>>(),
                logger,
                false);

            var composition = new Composition(wrapper.Services, typeLoader, Mock.Of<IProfilingLogger>(), Mock.Of<IRuntimeState>(), TestHelper.IOHelper, AppCaches.NoCache);

            composition.RegisterUnique<ILogger>(_ => Mock.Of<ILogger>());
            composition.RegisterUnique<ILoggerFactory>(_ => NullLoggerFactory.Instance);
            composition.RegisterUnique<IProfiler>(_ => Mock.Of<IProfiler>());

            composition.RegisterUnique(typeLoader);

            composition.WithCollectionBuilder<MapperCollectionBuilder>()
                .AddCoreMappers();

            composition.RegisterUnique<ISqlContext>(_ => SqlContext);

            var factory = Current.Factory = composition.CreateFactory();

            var pocoMappers = new NPoco.MapperCollection { new PocoMapper() };
            var pocoDataFactory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, pocoMappers).Init());
            var sqlSyntax = new SqlCeSyntaxProvider();
            SqlContext = new SqlContext(sqlSyntax, DatabaseType.SQLCe, pocoDataFactory, new Lazy<IMapperCollection>(() => factory.GetInstance<IMapperCollection>()));
            Mappers = factory.GetInstance<IMapperCollection>();

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
