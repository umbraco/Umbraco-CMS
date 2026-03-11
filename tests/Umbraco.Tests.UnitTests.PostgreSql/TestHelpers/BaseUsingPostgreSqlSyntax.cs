// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Our.Umbraco.PostgreSql;
using Our.Umbraco.PostgreSql.Services;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Extensions;
using IMapperCollection = Umbraco.Cms.Infrastructure.Persistence.Mappers.IMapperCollection;
using MapperCollection = NPoco.MapperCollection;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;

[TestFixture]
public abstract class BaseUsingPostgreSqlSyntax
{
    [SetUp]
    public virtual void Setup()
    {
        var container = TestHelper.GetServiceCollection();
        var typeLoader = TestHelper.GetMockedTypeLoader();

        var composition = new UmbracoBuilder(container, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        composition.WithCollectionBuilder<MapperCollectionBuilder>()
            .AddCoreMappers();

        composition.Services.AddUnique(_ => SqlContext);

        var factory = composition.CreateServiceProvider();
        var pocoMappers = new MapperCollection { new NullableDateMapper() };
        var pocoDataFactory =
            new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, pocoMappers).Init(), pocoMappers);
        var sqlSyntax = new PostgreSqlSyntaxProvider(Options.Create(new PostgreSqlOptions()), Mock.Of<IPackagesService>());
        SqlContext = new SqlContext(sqlSyntax, DatabaseType.SqlServer2012, pocoDataFactory, factory.GetRequiredService<IMapperCollection>());
        Mappers = factory.GetRequiredService<IMapperCollection>();
    }

    protected IMapperCollection Mappers { get; private set; }

    protected ISqlContext SqlContext { get; private set; }

    protected Sql<ISqlContext> Sql() => NPoco.Sql.BuilderFor(SqlContext);
}
