// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.UnitTests.TestHelpers;

namespace Umbraco.Tests.TestHelpers
{
    [TestFixture]
    public abstract class BaseUsingSqlSyntax
    {
        protected IMapperCollection Mappers { get; private set; }

        protected ISqlContext SqlContext { get; private set; }

        protected Sql<ISqlContext> Sql() => NPoco.Sql.BuilderFor(SqlContext);

        [SetUp]
        public virtual void Setup()
        {
            IServiceCollection container = TestHelper.GetServiceCollection();
            TypeLoader typeLoader = TestHelper.GetMockedTypeLoader();

            var composition = new UmbracoBuilder(container, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

            composition.WithCollectionBuilder<MapperCollectionBuilder>()
                .AddCoreMappers();

            composition.Services.AddUnique(_ => SqlContext);

            IServiceProvider factory = composition.CreateServiceProvider();
            var pocoMappers = new NPoco.MapperCollection { new PocoMapper() };
            var pocoDataFactory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, pocoMappers).Init(), pocoMappers);
            var sqlSyntax = new SqlServerSyntaxProvider();
            SqlContext = new SqlContext(sqlSyntax, DatabaseType.SqlServer2012, pocoDataFactory, new Lazy<IMapperCollection>(() => factory.GetRequiredService<IMapperCollection>()));
            Mappers = factory.GetRequiredService<IMapperCollection>();
        }
    }
}
