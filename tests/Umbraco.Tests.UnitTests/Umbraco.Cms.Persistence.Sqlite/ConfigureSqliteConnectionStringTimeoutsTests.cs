using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.Sqlite.Configuration;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.Sqlite;

[TestFixture]
public class ConfigureSqliteConnectionStringTimeoutsTests
{
    private const string ConnectionString = "Data Source=x.db;Cache=Shared;Foreign Keys=True;Pooling=True";

    private static string ProviderName => global::Umbraco.Cms.Persistence.Sqlite.Constants.ProviderName;

    [Test]
    public void Cannot_Change_Connection_String_When_No_Command_Timeout_Is_Configured()
    {
        ConnectionStrings options = PostConfigure(new GlobalSettings(), out _);

        Assert.AreEqual(ConnectionString, options.ConnectionString);
    }

    [Test]
    public void Can_Apply_Configured_Command_Timeout()
    {
        ConnectionStrings options = PostConfigure(
            new GlobalSettings { DatabaseCommandTimeout = TimeSpan.FromMinutes(5) },
            out _);

        Assert.AreEqual(300, new SqliteConnectionStringBuilder(options.ConnectionString).DefaultTimeout);
    }

    [Test]
    public void Cannot_Apply_Configured_Connect_Timeout_To_Sqlite()
    {
        ConnectionStrings options = PostConfigure(
            new GlobalSettings { DatabaseConnectTimeout = TimeSpan.FromMinutes(1) },
            out FakeLogger logger);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(ConnectionString, options.ConnectionString);
            Assert.AreEqual(1, logger.LogEntries.Count);
            Assert.AreEqual(LogLevel.Information, logger.LogEntries[0].Level);
            Assert.That(logger.LogEntries[0].Message, Does.Contain("DatabaseConnectTimeout"));
        });
    }

    [Test]
    public void Can_Report_Inapplicable_Connect_Timeout_Only_Once()
    {
        var globalSettings = new GlobalSettings { DatabaseConnectTimeout = TimeSpan.FromMinutes(1) };
        var logger = new FakeLogger();
        var sut = new ConfigureSqliteConnectionStringTimeouts(Options.Create(globalSettings), logger);

        for (var i = 0; i < 3; i++)
        {
            sut.PostConfigure(name: null, NewOptions(ConnectionString, ProviderName));
        }

        Assert.AreEqual(1, logger.LogEntries.Count);
    }

    [Test]
    public void Cannot_Change_Connection_String_For_Another_Provider()
    {
        const string sqlServerConnectionString = "Server=.;Database=x";

        ConnectionStrings options = PostConfigure(
            new GlobalSettings { DatabaseCommandTimeout = TimeSpan.FromMinutes(5) },
            out _,
            sqlServerConnectionString,
            providerName: "Microsoft.Data.SqlClient");

        Assert.AreEqual(sqlServerConnectionString, options.ConnectionString);
    }

    [Test]
    public void Can_Apply_Configured_Command_Timeout_For_A_Case_Variant_Provider_Name()
    {
        ConnectionStrings options = PostConfigure(
            new GlobalSettings { DatabaseCommandTimeout = TimeSpan.FromMinutes(5) },
            out _,
            providerName: "microsoft.data.sqlite");

        Assert.AreEqual(300, new SqliteConnectionStringBuilder(options.ConnectionString).DefaultTimeout);
    }

    [Test]
    public void Cannot_Change_A_Connection_String_That_Cannot_Be_Parsed()
    {
        const string unparseableConnectionString = "Data Source=x.db;NotASqliteKeyword=1";

        ConnectionStrings options = PostConfigure(
            new GlobalSettings { DatabaseCommandTimeout = TimeSpan.FromMinutes(5) },
            out FakeLogger logger,
            unparseableConnectionString);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(unparseableConnectionString, options.ConnectionString);
            Assert.AreEqual(1, logger.LogEntries.Count);
            Assert.AreEqual(LogLevel.Warning, logger.LogEntries[0].Level);
        });
    }

    private static ConnectionStrings NewOptions(string connectionString, string providerName)
        => new() { ConnectionString = connectionString, ProviderName = providerName };

    private static ConnectionStrings PostConfigure(
        GlobalSettings globalSettings,
        out FakeLogger logger,
        string connectionString = ConnectionString,
        string? providerName = null)
    {
        ConnectionStrings options = NewOptions(connectionString, providerName ?? ProviderName);
        logger = new FakeLogger();

        new ConfigureSqliteConnectionStringTimeouts(Options.Create(globalSettings), logger)
            .PostConfigure(name: null, options);

        return options;
    }

    private sealed class FakeLogger : ILogger<ConfigureSqliteConnectionStringTimeouts>
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
