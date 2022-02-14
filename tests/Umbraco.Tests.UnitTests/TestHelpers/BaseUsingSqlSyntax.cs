// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.TestHelpers
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
            var pocoMappers = new NPoco.MapperCollection
            {
                new NullableDateMapper()
            };
            var pocoDataFactory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, pocoMappers).Init(), pocoMappers);
            var sqlSyntax = new SqlServerSyntaxProvider(Options.Create(new GlobalSettings()));
            SqlContext = new SqlContext(sqlSyntax, DatabaseType.SqlServer2012, pocoDataFactory, factory.GetRequiredService<IMapperCollection>());
            Mappers = factory.GetRequiredService<IMapperCollection>();
        }
    }
}
