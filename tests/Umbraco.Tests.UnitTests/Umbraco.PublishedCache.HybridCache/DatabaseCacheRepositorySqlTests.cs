using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
internal sealed class DatabaseCacheRepositorySqlTests : BaseUsingSqlSyntax
{
    private DatabaseCacheRepository _repository = null!;

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        var databaseFactory = new Mock<IUmbracoDatabaseFactory>();
        databaseFactory.SetupGet(x => x.SqlContext).Returns(SqlContext);

        _repository = new DatabaseCacheRepository(
            Mock.Of<IScopeAccessor>(),
            databaseFactory.Object,
            AppCaches.NoCache,
            NullLogger<DatabaseCacheRepository>.Instance,
            Mock.Of<IShortStringHelper>(),
            new UrlSegmentProviderCollection(() => []),
            Mock.Of<IContentCacheDataSerializerFactory>(),
            Options.Create(new NuCacheSettings()));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Content_Query_Binds_Each_Join_Condition_To_The_Value_It_Compares_With(bool batched)
    {
        Sql<ISqlContext> sql = batched
            ? _repository.GetContentSourcesSql([Guid.NewGuid(), Guid.NewGuid()])
            : _repository.GetContentSourceSql(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.That(BoundValue(sql, "[umbracoContentVersion].[current]"), Is.EqualTo(true));
            Assert.That(BoundValue(sql, "[pdver].[published]"), Is.EqualTo(true));
            Assert.That(BoundValue(sql, "[nuEdit].[published]"), Is.EqualTo(false));
            Assert.That(BoundValue(sql, "[nuPub].[published]"), Is.EqualTo(true));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Media_Query_Binds_Each_Join_Condition_To_The_Value_It_Compares_With(bool batched)
    {
        Sql<ISqlContext> sql = batched
            ? _repository.GetMediaSourcesSql([Guid.NewGuid(), Guid.NewGuid()])
            : _repository.GetMediaSourceSql(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.That(BoundValue(sql, "[umbracoContentVersion].[current]"), Is.EqualTo(true));
            Assert.That(BoundValue(sql, "[nuEdit].[published]"), Is.EqualTo(false));
        });
    }

    [Test]
    public void Query_Built_Again_Binds_The_Same_Values()
    {
        Sql<ISqlContext> first = _repository.GetContentSourceSql(Guid.NewGuid());
        Sql<ISqlContext> second = _repository.GetContentSourceSql(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.That(second.SQL, Is.EqualTo(first.SQL));
            Assert.That(second.Arguments.Take(4), Is.EqualTo(first.Arguments.Take(4)));
        });
    }

    // The value a condition compares its column with: the argument the condition's placeholder in the built SQL refers to.
    private static object BoundValue(Sql<ISqlContext> sql, string column)
    {
        MatchCollection matches = Regex.Matches(sql.SQL, $@"{Regex.Escape(column)}\s*=\s*@(\d+)");
        Assert.That(matches, Has.Count.EqualTo(1), $"Expected one condition on {column} in: {sql.SQL}");
        return sql.Arguments[int.Parse(matches[0].Groups[1].Value)];
    }
}
