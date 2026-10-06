using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NPoco.DatabaseTypes;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

[TestFixture]
public class EnsureLongCommandTimeoutTests
{
    private const int MinimumCommandTimeoutInSeconds = 300;

    [Test]
    public void Cannot_Lower_Longer_Command_Timeout_From_Connection_String()
    {
        using UmbracoDatabase database = CreateDatabase("Data Source=x.db;Default Timeout=600");

        TestMigration.Invoke(database);

        Assert.AreEqual(0, database.CommandTimeout);
    }

    [Test]
    public void Cannot_Lower_Command_Timeout_Of_No_Limit_From_Connection_String()
    {
        using UmbracoDatabase database = CreateDatabase("Data Source=x.db;Default Timeout=0");

        TestMigration.Invoke(database);

        Assert.AreEqual(0, database.CommandTimeout);
    }

    [Test]
    public void Can_Raise_Shorter_Command_Timeout_From_Connection_String()
    {
        using UmbracoDatabase database = CreateDatabase("Data Source=x.db;Default Timeout=60");

        TestMigration.Invoke(database);

        Assert.AreEqual(MinimumCommandTimeoutInSeconds, database.CommandTimeout);
    }

    [Test]
    public void Can_Raise_Provider_Default_Command_Timeout()
    {
        using UmbracoDatabase database = CreateDatabase("Data Source=x.db");

        TestMigration.Invoke(database);

        Assert.AreEqual(MinimumCommandTimeoutInSeconds, database.CommandTimeout);
    }

    [Test]
    public void Cannot_Lower_Longer_Command_Timeout_Set_In_Code()
    {
        var database = new Mock<IDatabase>();
        database.SetupGet(x => x.CommandTimeout).Returns(500);

        TestMigration.Invoke(database.Object);

        database.VerifySet(x => x.CommandTimeout = It.IsAny<int>(), Times.Never);
    }

    [Test]
    public void Can_Raise_Shorter_Command_Timeout_Set_In_Code()
    {
        var database = new Mock<IDatabase>();
        database.SetupGet(x => x.CommandTimeout).Returns(60);

        TestMigration.Invoke(database.Object);

        database.VerifySet(x => x.CommandTimeout = MinimumCommandTimeoutInSeconds, Times.Once);
    }

    [Test]
    public void Can_Raise_Unset_Command_Timeout_When_Effective_Timeout_Is_Unknown()
    {
        var database = new Mock<IDatabase>();
        database.SetupGet(x => x.CommandTimeout).Returns(0);

        TestMigration.Invoke(database.Object);

        database.VerifySet(x => x.CommandTimeout = MinimumCommandTimeoutInSeconds, Times.Once);
    }

    private static UmbracoDatabase CreateDatabase(string connectionString)
    {
        var sqlSyntax = new Mock<ISqlSyntaxProvider>();
        sqlSyntax.SetupGet(x => x.DefaultIsolationLevel).Returns(IsolationLevel.ReadCommitted);

        var sqlContext = new Mock<ISqlContext>();
        sqlContext.SetupGet(x => x.DatabaseType).Returns(new SQLiteDatabaseType());
        sqlContext.SetupGet(x => x.SqlSyntax).Returns(sqlSyntax.Object);

        return new UmbracoDatabase(
            connectionString,
            sqlContext.Object,
            SqliteFactory.Instance,
            NullLogger<UmbracoDatabase>.Instance,
            null,
            new DatabaseSchemaCreatorFactory(
                NullLogger<DatabaseSchemaCreator>.Instance,
                NullLoggerFactory.Instance,
                Mock.Of<IUmbracoVersion>(),
                Mock.Of<IEventAggregator>(),
                Mock.Of<IOptionsMonitor<InstallDefaultDataSettings>>()));
    }

    private sealed class TestMigration : AsyncMigrationBase
    {
        private TestMigration()
            : base(Mock.Of<IMigrationContext>())
        {
        }

        public static void Invoke(IDatabase database) => EnsureLongCommandTimeout(database);

        protected override Task MigrateAsync() => Task.CompletedTask;
    }
}
