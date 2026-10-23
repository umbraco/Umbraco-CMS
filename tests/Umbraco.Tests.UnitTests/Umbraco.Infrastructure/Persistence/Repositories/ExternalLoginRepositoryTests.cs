// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
internal sealed class ExternalLoginRepositoryTests : BaseUsingSqlSyntax
{
    [Test]
    public void Saving_Tokens_Locks_The_Existing_Tokens_Through_The_Syntax_Provider()
    {
        const string loginProvider = "Test.Provider";
        var userOrMemberKey = Guid.NewGuid();
        var sqlContext = new SqlContext(new TrailingLockSqlSyntaxProvider(), DatabaseType.SqlServer2012, SqlContext.PocoDataFactory, Mappers);

        var database = new Mock<IUmbracoDatabase>();
        database.Setup(x => x.SqlContext).Returns(sqlContext);
        database.Setup(x => x.Fetch<ExternalLoginDto>(It.IsAny<Sql>()))
            .Returns([new ExternalLoginDto { Id = 1, LoginProvider = loginProvider, ProviderKey = "key", UserOrMemberKey = userOrMemberKey }]);
        Sql? tokenQuery = null;
        database.Setup(x => x.Fetch<ExternalLoginTokenDto>(It.IsAny<Sql>()))
            .Callback<Sql>(sql => tokenQuery = sql)
            .Returns([]);

        var scope = new Mock<IScope>();
        scope.Setup(x => x.Database).Returns(database.Object);
        scope.Setup(x => x.SqlContext).Returns(sqlContext);
        var repository = new ExternalLoginRepository(
            Mock.Of<IScopeAccessor>(x => x.AmbientScope == scope.Object),
            AppCaches.NoCache,
            NullLogger<ExternalLoginRepository>.Instance,
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>());

        repository.Save(userOrMemberKey, [new ExternalLoginToken(loginProvider, "token", "value")]);

        Assert.That(tokenQuery?.SQL.NoCrLf(), Does.EndWith(TrailingLockSqlSyntaxProvider.LockClause));
    }

    // Stands for databases that lock the selected rows with a clause after the query rather than with a table hint.
    private sealed class TrailingLockSqlSyntaxProvider : SqlServerSyntaxProvider
    {
        public const string LockClause = "FOR UPDATE";

        public TrailingLockSqlSyntaxProvider()
            : base(Options.Create(new GlobalSettings()))
        {
        }

        public override Sql<ISqlContext> InsertForUpdateHint(Sql<ISqlContext> sql) => sql.Append(LockClause);

        public override Sql<ISqlContext> AppendForUpdateHint(Sql<ISqlContext> sql) => sql;
    }
}
