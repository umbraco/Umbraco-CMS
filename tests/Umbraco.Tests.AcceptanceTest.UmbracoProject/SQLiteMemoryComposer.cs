using Microsoft.Data.Sqlite;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace UmbracoProject;

/// <summary>
/// Ensures a SQLite in-memory database is persisted for the whole application duration.
/// </summary>
public sealed class SQLiteMemoryComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        var connectionString = builder.Config.GetUmbracoConnectionString(out var providerName);
        if (!string.IsNullOrEmpty(connectionString) &&
            Constants.ProviderNames.SQLLite.InvariantEquals(providerName) &&
            connectionString.InvariantContains("Mode=Memory"))
        {
            // Open new SQLite connection to ensure in-memory database is persisted for the whole application duration
            var connection = new SqliteConnection(connectionString);
            connection.Open();

            // And ensure connection is kept open (by keeping a reference) and gets gracefully closed/disposed when application stops
            builder.Services.AddHostedService(_ => new SQLiteMemoryHostedService(connection));
        }
    }

    private sealed class SQLiteMemoryHostedService : IHostedService, IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public SQLiteMemoryHostedService(SqliteConnection connection) => _connection = connection;

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task StopAsync(CancellationToken cancellationToken) => await _connection.CloseAsync();

        public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
    }
}
