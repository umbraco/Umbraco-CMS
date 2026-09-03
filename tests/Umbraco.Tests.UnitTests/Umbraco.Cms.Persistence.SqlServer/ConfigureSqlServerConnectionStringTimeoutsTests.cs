using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.SqlServer.Configuration;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.SqlServer;

[TestFixture]
public class ConfigureSqlServerConnectionStringTimeoutsTests
{
    private const string ConnectionString = "Server=.;Database=x";

    [Test]
    public void Cannot_Change_Connection_String_When_Neither_Timeout_Is_Configured()
    {
        ConnectionStrings options = PostConfigure(new GlobalSettings());

        Assert.That(options.ConnectionString, Is.EqualTo(ConnectionString));
    }

    [Test]
    public void Can_Apply_Configured_Command_Timeout()
    {
        ConnectionStrings options = PostConfigure(new GlobalSettings
        {
            DatabaseCommandTimeout = TimeSpan.FromMinutes(5),
        });

        Assert.That(new SqlConnectionStringBuilder(options.ConnectionString).CommandTimeout, Is.EqualTo(300));
    }

    [Test]
    public void Can_Apply_Configured_Connect_Timeout()
    {
        ConnectionStrings options = PostConfigure(new GlobalSettings
        {
            DatabaseConnectTimeout = TimeSpan.FromMinutes(1),
        });

        Assert.That(new SqlConnectionStringBuilder(options.ConnectionString).ConnectTimeout, Is.EqualTo(60));
    }

    [Test]
    public void Can_Apply_Both_Timeouts_Independently()
    {
        ConnectionStrings options = PostConfigure(new GlobalSettings
        {
            DatabaseCommandTimeout = TimeSpan.FromMinutes(5),
            DatabaseConnectTimeout = TimeSpan.FromSeconds(10),
        });

        var connectionStringBuilder = new SqlConnectionStringBuilder(options.ConnectionString);

        Assert.Multiple(() =>
        {
            Assert.That(connectionStringBuilder.CommandTimeout, Is.EqualTo(300));
            Assert.That(connectionStringBuilder.ConnectTimeout, Is.EqualTo(10));
        });
    }

    [Test]
    public void Can_Take_Precedence_Over_Connection_String()
    {
        ConnectionStrings options = PostConfigure(
            new GlobalSettings { DatabaseCommandTimeout = TimeSpan.FromMinutes(5) },
            "Server=.;Database=x;Command Timeout=30");

        Assert.That(new SqlConnectionStringBuilder(options.ConnectionString).CommandTimeout, Is.EqualTo(300));
    }

    [Test]
    public void Can_Apply_Command_Timeout_Of_No_Limit()
    {
        ConnectionStrings options = PostConfigure(new GlobalSettings
        {
            DatabaseCommandTimeout = TimeSpan.Zero,
        });

        Assert.That(new SqlConnectionStringBuilder(options.ConnectionString).CommandTimeout, Is.EqualTo(0));
    }

    [Test]
    public void Cannot_Change_Connection_String_For_Another_Provider()
    {
        const string sqliteConnectionString = "Data Source=x.db";

        ConnectionStrings options = PostConfigure(
            new GlobalSettings { DatabaseCommandTimeout = TimeSpan.FromMinutes(5) },
            sqliteConnectionString,
            providerName: "Microsoft.Data.Sqlite");

        Assert.That(options.ConnectionString, Is.EqualTo(sqliteConnectionString));
    }

    [Test]
    public void Can_Apply_Configured_Command_Timeout_For_A_Case_Variant_Provider_Name()
    {
        ConnectionStrings options = PostConfigure(
            new GlobalSettings { DatabaseCommandTimeout = TimeSpan.FromMinutes(5) },
            providerName: "microsoft.data.sqlclient");

        Assert.That(new SqlConnectionStringBuilder(options.ConnectionString).CommandTimeout, Is.EqualTo(300));
    }

    [Test]
    public void Cannot_Change_A_Connection_String_That_Cannot_Be_Parsed()
    {
        const string unparseableConnectionString = "Server=.;Database=x;NotASqlServerKeyword=1";
        var logger = new FakeLogger();
        var options = new ConnectionStrings
        {
            ConnectionString = unparseableConnectionString,
            ProviderName = global::Umbraco.Cms.Persistence.SqlServer.Constants.ProviderName,
        };

        new ConfigureSqlServerConnectionStringTimeouts(
                Options.Create(new GlobalSettings { DatabaseCommandTimeout = TimeSpan.FromMinutes(5) }),
                logger)
            .PostConfigure(name: null, options);

        Assert.Multiple(() =>
        {
            Assert.That(options.ConnectionString, Is.EqualTo(unparseableConnectionString));
            Assert.That(logger.LogEntries, Has.Count.EqualTo(1));
            Assert.That(logger.LogEntries[0].Level, Is.EqualTo(LogLevel.Warning));
        });
    }

    [Test]
    public void Cannot_Change_Connection_String_When_A_Timeout_Is_Rejected_By_The_Builder()
    {
        // Validation rejects a negative timeout before it reaches here, so this only asserts that applying the
        // timeouts is covered by the same guard as parsing the connection string.
        var logger = new FakeLogger();
        var options = new ConnectionStrings
        {
            ConnectionString = ConnectionString,
            ProviderName = global::Umbraco.Cms.Persistence.SqlServer.Constants.ProviderName,
        };

        new ConfigureSqlServerConnectionStringTimeouts(
                Options.Create(new GlobalSettings { DatabaseCommandTimeout = TimeSpan.FromSeconds(-1) }),
                logger)
            .PostConfigure(name: null, options);

        Assert.Multiple(() =>
        {
            Assert.That(options.ConnectionString, Is.EqualTo(ConnectionString));
            Assert.That(logger.LogEntries, Has.Count.EqualTo(1));
            Assert.That(logger.LogEntries[0].Level, Is.EqualTo(LogLevel.Warning));
        });
    }

    private static ConnectionStrings PostConfigure(
        GlobalSettings globalSettings,
        string connectionString = ConnectionString,
        string? providerName = null)
    {
        var options = new ConnectionStrings
        {
            ConnectionString = connectionString,
            ProviderName = providerName ?? global::Umbraco.Cms.Persistence.SqlServer.Constants.ProviderName,
        };

        new ConfigureSqlServerConnectionStringTimeouts(Options.Create(globalSettings), new FakeLogger())
            .PostConfigure(name: null, options);

        return options;
    }

    private sealed class FakeLogger : ILogger<ConfigureSqlServerConnectionStringTimeouts>
    {
        public List<LogEntry> LogEntries { get; } = [];

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => LogEntries.Add(new LogEntry(logLevel, formatter(state, exception)));

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public record LogEntry(LogLevel Level, string Message);
    }
}
