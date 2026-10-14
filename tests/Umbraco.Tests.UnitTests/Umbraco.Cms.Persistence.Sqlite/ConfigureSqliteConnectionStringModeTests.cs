using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.Sqlite.Configuration;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.Sqlite;

[TestFixture]
public class ConfigureSqliteConnectionStringModeTests
{
    private const string ConnectionString = "Data Source=x.db;Mode=ReadWriteCreate;Foreign Keys=True;Pooling=True";

    private static string ProviderName => global::Umbraco.Cms.Persistence.Sqlite.Constants.ProviderName;

    [Test]
    public void Can_Downgrade_Read_Write_Create_To_Read_Write()
    {
        ConnectionStrings options = PostConfigure(out _);

        Assert.AreEqual(
            SqliteOpenMode.ReadWrite,
            new SqliteConnectionStringBuilder(options.ConnectionString).Mode);
    }

    [Test]
    public void Can_Downgrade_Read_Write_Create_For_A_Case_Variant_Provider_Name()
    {
        ConnectionStrings options = PostConfigure(out _, providerName: "microsoft.data.sqlite");

        Assert.AreEqual(
            SqliteOpenMode.ReadWrite,
            new SqliteConnectionStringBuilder(options.ConnectionString).Mode);
    }

    [Test]
    public void Can_Downgrade_An_Implicit_Read_Write_Create()
    {
        // ReadWriteCreate is the default, so a connection string without an explicit mode creates as well.
        ConnectionStrings options = PostConfigure(out _, "Data Source=x.db;Foreign Keys=True;Pooling=True");

        Assert.AreEqual(
            SqliteOpenMode.ReadWrite,
            new SqliteConnectionStringBuilder(options.ConnectionString).Mode);
    }

    [TestCase("Data Source=x.db;Mode=ReadOnly")]
    [TestCase("Data Source=x;Mode=Memory;Cache=Shared")]
    public void Cannot_Change_A_Connection_String_That_Does_Not_Create(string connectionString)
    {
        ConnectionStrings options = PostConfigure(out _, connectionString);

        Assert.AreEqual(connectionString, options.ConnectionString);
    }

    [Test]
    public void Cannot_Change_Connection_String_For_Another_Provider()
    {
        const string sqlServerConnectionString = "Server=.;Database=x";

        ConnectionStrings options = PostConfigure(
            out _,
            sqlServerConnectionString,
            providerName: "Microsoft.Data.SqlClient");

        Assert.AreEqual(sqlServerConnectionString, options.ConnectionString);
    }

    [Test]
    public void Cannot_Change_An_Empty_Connection_String()
    {
        ConnectionStrings options = PostConfigure(out _, string.Empty);

        Assert.AreEqual(string.Empty, options.ConnectionString);
    }

    [Test]
    public void Cannot_Change_A_Connection_String_That_Cannot_Be_Parsed()
    {
        const string unparseableConnectionString = "Data Source=x.db;NotASqliteKeyword=1";

        ConnectionStrings options = PostConfigure(out FakeLogger logger, unparseableConnectionString);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(unparseableConnectionString, options.ConnectionString);
            Assert.AreEqual(1, logger.LogEntries.Count);
            Assert.AreEqual(LogLevel.Warning, logger.LogEntries[0].Level);
        });
    }

    private static ConnectionStrings PostConfigure(
        out FakeLogger logger,
        string connectionString = ConnectionString,
        string? providerName = null)
    {
        var options = new ConnectionStrings
        {
            ConnectionString = connectionString,
            ProviderName = providerName ?? ProviderName,
        };
        logger = new FakeLogger();

        new ConfigureSqliteConnectionStringMode(logger).PostConfigure(name: null, options);

        return options;
    }

    private sealed class FakeLogger : ILogger<ConfigureSqliteConnectionStringMode>
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
