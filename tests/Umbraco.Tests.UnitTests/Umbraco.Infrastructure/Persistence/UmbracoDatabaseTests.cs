using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NPoco.DatabaseTypes;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence;

[TestFixture]
public class UmbracoDatabaseTests
{
    [Test]
    public void Can_Leave_Command_Timeout_To_Provider_When_Connection_String_Configures_One()
    {
        // The connect timeout has to be present for this to have teeth: with "Command Timeout" alone the
        // pre-fix code found no keyword it recognised and left CommandTimeout at zero anyway.
        using UmbracoDatabase database = CreateSqlServerDatabase("Server=.;Database=x;Connect Timeout=15;Command Timeout=300");

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, database.CommandTimeout);
            Assert.AreEqual(300, database.EffectiveCommandTimeout);
        });
    }

    [Test]
    public void Can_Preserve_Command_Timeout_Of_No_Limit_From_Connection_String()
    {
        using UmbracoDatabase database = CreateSqlServerDatabase("Server=.;Database=x;Command Timeout=0;Connect Timeout=60");

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, database.CommandTimeout);
            Assert.AreEqual(0, database.EffectiveCommandTimeout);
        });
    }

    [Test]
    public void Can_Apply_Connect_Timeout_As_Command_Timeout_When_None_Configured()
    {
        using UmbracoDatabase database = CreateSqlServerDatabase("Server=.;Database=x;Connect Timeout=60");

        Assert.Multiple(() =>
        {
            Assert.AreEqual(60, database.CommandTimeout);
            Assert.AreEqual(60, database.EffectiveCommandTimeout);
        });
    }

    [Test]
    public void Can_Fall_Back_To_Provider_Default_When_No_Timeout_Configured()
    {
        using UmbracoDatabase database = CreateSqlServerDatabase("Server=.;Database=x");

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, database.CommandTimeout);
            Assert.AreEqual(30, database.EffectiveCommandTimeout);
        });
    }

    [Test]
    public void Can_Report_Command_Timeout_From_Sqlite_Connection_String()
    {
        using UmbracoDatabase database = CreateSqliteDatabase("Data Source=x.db;Default Timeout=600");

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, database.CommandTimeout);
            Assert.AreEqual(600, database.EffectiveCommandTimeout);
        });
    }

    [Test]
    public void Can_Report_Command_Timeout_Set_In_Code_In_Preference_To_Connection_String()
    {
        using UmbracoDatabase database = CreateSqliteDatabase("Data Source=x.db;Default Timeout=600");
        database.CommandTimeout = 45;

        Assert.AreEqual(45, database.EffectiveCommandTimeout);
    }

    [Test]
    public void Can_Execute_Commands_With_Timeout_From_Connection_String()
    {
        // Exercises the whole mechanism against a real database, pinning the NPoco behaviour the fix relies
        // on: a zero Database.CommandTimeout leaves the provider's value on the command.
        using var database = new TimeoutCapturingDatabase(
            "Data Source=:memory:;Default Timeout=600",
            CreateSqlContext(new SQLiteDatabaseType()),
            SqliteFactory.Instance,
            NullLogger<UmbracoDatabase>.Instance,
            null,
            CreateSchemaCreatorFactory());

        database.ExecuteScalar<long>("SELECT 1");

        Assert.AreEqual(600, database.AppliedCommandTimeout);
    }

    [Test]
    public void Can_Execute_Commands_With_Timeout_Set_In_Code_In_Preference_To_Connection_String()
    {
        using var database = new TimeoutCapturingDatabase(
            "Data Source=:memory:;Default Timeout=600",
            CreateSqlContext(new SQLiteDatabaseType()),
            SqliteFactory.Instance,
            NullLogger<UmbracoDatabase>.Instance,
            null,
            CreateSchemaCreatorFactory());
        database.CommandTimeout = 45;

        database.ExecuteScalar<long>("SELECT 1");

        Assert.AreEqual(45, database.AppliedCommandTimeout);
    }

    private static UmbracoDatabase CreateSqlServerDatabase(string connectionString)
        => CreateDatabase(connectionString, SqlClientFactory.Instance, new UmbracoSqlServerDatabaseType());

    private static UmbracoDatabase CreateSqliteDatabase(string connectionString)
        => CreateDatabase(connectionString, SqliteFactory.Instance, new SQLiteDatabaseType());

    private static UmbracoDatabase CreateDatabase(string connectionString, DbProviderFactory provider, DatabaseType databaseType)
        => new(
            connectionString,
            CreateSqlContext(databaseType),
            provider,
            NullLogger<UmbracoDatabase>.Instance,
            null,
            CreateSchemaCreatorFactory());

    private static ISqlContext CreateSqlContext(DatabaseType databaseType)
    {
        var sqlSyntax = new Mock<ISqlSyntaxProvider>();
        sqlSyntax.SetupGet(x => x.DefaultIsolationLevel).Returns(IsolationLevel.ReadCommitted);

        var sqlContext = new Mock<ISqlContext>();
        sqlContext.SetupGet(x => x.DatabaseType).Returns(databaseType);
        sqlContext.SetupGet(x => x.SqlSyntax).Returns(sqlSyntax.Object);

        return sqlContext.Object;
    }

    private static DatabaseSchemaCreatorFactory CreateSchemaCreatorFactory()
        => new(
            NullLogger<DatabaseSchemaCreator>.Instance,
            NullLoggerFactory.Instance,
            Mock.Of<IUmbracoVersion>(),
            Mock.Of<IEventAggregator>(),
            Mock.Of<IOptionsMonitor<InstallDefaultDataSettings>>());

    private sealed class TimeoutCapturingDatabase : UmbracoDatabase
    {
        public TimeoutCapturingDatabase(
            string connectionString,
            ISqlContext sqlContext,
            DbProviderFactory provider,
            ILogger<UmbracoDatabase> logger,
            IBulkSqlInsertProvider? bulkSqlInsertProvider,
            DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory)
            : base(connectionString, sqlContext, provider, logger, bulkSqlInsertProvider, databaseSchemaCreatorFactory)
        {
        }

        public int? AppliedCommandTimeout { get; private set; }

        protected override void OnExecutingCommand(DbCommand cmd)
        {
            base.OnExecutingCommand(cmd);
            AppliedCommandTimeout = cmd.CommandTimeout;
        }
    }
}
