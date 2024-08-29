using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace UmbracoProject;

/// <summary>
/// Disable waiting on log IO to finish when commiting a transaction (we can tolerate some data loss) on SQL Server.
/// </summary>
public sealed class SqlServerDelayedDurabilityComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        var connectionString = builder.Config.GetUmbracoConnectionString(out var providerName);
        if (!string.IsNullOrEmpty(connectionString) &&
            Constants.ProviderNames.SQLServer.InvariantEquals(providerName))
        {
            builder.AddNotificationAsyncHandler<UnattendedInstallNotification, SqlServerDelayedDurabilityInstallNotification>();
        }
    }

    private sealed class SqlServerDelayedDurabilityInstallNotification : INotificationAsyncHandler<UnattendedInstallNotification>
    {
        private readonly IOptions<ConnectionStrings> _connectionStrings;

        public SqlServerDelayedDurabilityInstallNotification(IOptions<ConnectionStrings> connectionStrings) => _connectionStrings = connectionStrings;

        public async Task HandleAsync(UnattendedInstallNotification notification, CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionStrings.Value.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            // Disable waiting on log IO to finish when commiting a transaction (we can tolerate some data loss)
            var command = new SqlCommand("ALTER DATABASE CURRENT SET DELAYED_DURABILITY = FORCED;", connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
