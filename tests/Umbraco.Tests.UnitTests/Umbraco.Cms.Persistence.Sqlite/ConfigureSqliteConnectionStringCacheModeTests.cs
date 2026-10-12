using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.Sqlite.Configuration;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.Sqlite;

[TestFixture]
public class ConfigureSqliteConnectionStringCacheModeTests
{
    private const string ConnectionString = "Data Source=x.db;Cache=Shared;Foreign Keys=True;Pooling=True";

    private static string ProviderName => global::Umbraco.Cms.Persistence.Sqlite.Constants.ProviderName;

    [Test]
    public void Can_Remove_Shared_Cache_Mode()
    {
        ConnectionStrings options = PostConfigure(out FakeLogger logger);

        Assert.Multiple(() =>
        {
            Assert.AreEqual("Data Source=x.db;Foreign Keys=True;Pooling=True", options.ConnectionString);
            Assert.AreEqual(1, logger.LogEntries.Count);
            Assert.AreEqual(LogLevel.Information, logger.LogEntries[0].Level);
        });
    }

    [Test]
    public void Can_Remove_Shared_Cache_Mode_For_A_Case_Variant_Provider_Name()
    {
        ConnectionStrings options = PostConfigure(out _, providerName: "microsoft.data.sqlite");

        Assert.AreEqual("Data Source=x.db;Foreign Keys=True;Pooling=True", options.ConnectionString);
    }

    [Test]
    public void Can_Report_Removed_Shared_Cache_Mode_Only_Once()
    {
        var logger = new FakeLogger();
        var sut = new ConfigureSqliteConnectionStringCacheMode(logger);

        for (var i = 0; i < 3; i++)
        {
            sut.PostConfigure(name: null, NewOptions(ConnectionString, ProviderName));
        }

        Assert.AreEqual(1, logger.LogEntries.Count);
    }

    [Test]
    public void Cannot_Change_A_Connection_String_Without_Shared_Cache_Mode()
    {
        const string connectionString = "Data Source=x.db;Foreign Keys=True;Pooling=True";

        ConnectionStrings options = PostConfigure(out FakeLogger logger, connectionString);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(connectionString, options.ConnectionString);
            Assert.IsEmpty(logger.LogEntries);
        });
    }

    [Test]
    public void Cannot_Remove_Shared_Cache_Mode_From_An_In_Memory_Database()
    {
        // Shared-cache mode is what makes an in-memory database shareable between connections.
        const string connectionString = "Data Source=x;Mode=Memory;Cache=Shared";

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

    private static ConnectionStrings NewOptions(string connectionString, string providerName)
        => new() { ConnectionString = connectionString, ProviderName = providerName };

    private static ConnectionStrings PostConfigure(
        out FakeLogger logger,
        string connectionString = ConnectionString,
        string? providerName = null)
    {
        ConnectionStrings options = NewOptions(connectionString, providerName ?? ProviderName);
        logger = new FakeLogger();

        new ConfigureSqliteConnectionStringCacheMode(logger).PostConfigure(name: null, options);

        return options;
    }

    private sealed class FakeLogger : ILogger<ConfigureSqliteConnectionStringCacheMode>
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
