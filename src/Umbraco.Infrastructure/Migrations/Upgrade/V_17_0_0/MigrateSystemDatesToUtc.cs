using NPoco;
using Umbraco.Cms.Infrastructure.Scoping;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

public class MigrateSystemDatesToUtc : UnscopedMigrationBase
{
    private readonly IScopeProvider _scopeProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IOptions<SystemDateMigrationSettings> _migrationSettings;
    private readonly ILogger<MigrateSystemDatesToUtc> _logger;

    public MigrateSystemDatesToUtc(
        IMigrationContext context,
        IScopeProvider scopeProvider,
        TimeProvider timeProvider,
        IOptions<SystemDateMigrationSettings> migrationSettings,
        ILogger<MigrateSystemDatesToUtc> logger)
        : base(context)
    {
        _scopeProvider = scopeProvider;
        _timeProvider = timeProvider;
        _migrationSettings = migrationSettings;
        _logger = logger;
    }

    protected override void Migrate()
    {
        if (_migrationSettings.Value.Enabled is false)
        {
            // If the migration is disabled, we do nothing.
            _logger.LogInformation("Skipping migration {MigrationName} as it is disabled in the configuration.", nameof(MigrateSystemDatesToUtc));
            Context.Complete();
            return;
        }

        // If the local server timezone is not set, we detect it.
        var timeZoneName = _migrationSettings.Value.LocalServerTimeZone;
        if (string.IsNullOrWhiteSpace(timeZoneName))
        {
            timeZoneName = _timeProvider.LocalTimeZone.StandardName;
            _logger.LogInformation("Migrating system dates to UTC using the detected local server timezone: {TimeZoneName}", timeZoneName);
        }
        else
        {
            _logger.LogInformation("Migrating system dates to UTC using the configured local server timezone: {TimeZoneName}", timeZoneName);
        }

        // If the local server timezone is UTC, skip the migration.
        if (string.Equals(timeZoneName, "Coordinated Universal Time", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Skipping migration {MigrationName} as the local server timezone is UTC.", nameof(MigrateSystemDatesToUtc));
            Context.Complete();
            return;
        }

        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();

        MigrateDateColumn(scope, "cmsMember", "emailConfirmedDate", timeZoneName);
        MigrateDateColumn(scope, "cmsMember", "lastLoginDate", timeZoneName);
        MigrateDateColumn(scope, "cmsMember", "lastLockoutDate", timeZoneName);
        MigrateDateColumn(scope, "cmsMember", "lastPasswordChangeDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoAccess", "createDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoAccess", "updateDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoAccessRule", "createDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoAccessRule", "updateDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoAudit", "eventDateUtc", timeZoneName);
        MigrateDateColumn(scope, "umbracoCreatedPackageSchema", "updateDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoContentVersion", "versionDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoContentVersionCleanupPolicy", "updated", timeZoneName);
        MigrateDateColumn(scope, "umbracoContentVersionCultureVariation", "date", timeZoneName);
        MigrateDateColumn(scope, "umbracoExternalLogin", "createDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoExternalLoginToken", "createDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoKeyValue", "update", timeZoneName);
        MigrateDateColumn(scope, "umbracoLog", "Datestamp", timeZoneName);
        MigrateDateColumn(scope, "umbracoNode", "createDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoRelation", "datetime", timeZoneName);
        MigrateDateColumn(scope, "umbracoServer", "registeredDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoServer", "lastNotifiedDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoUser", "createDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoUser", "updateDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoUser", "emailConfirmedDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoUser", "lastLockoutDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoUser", "lastPasswordChangeDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoUser", "lastLoginDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoUser", "invitedDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoUserGroup", "createDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoUserGroup", "updateDate", timeZoneName);
        MigrateDateColumn(scope, "umbracoWebhookLog", "date", timeZoneName);

        scope.Complete();
        Context.Complete();
    }

    private void MigrateDateColumn(IScope scope, string tableName, string columName, string timezoneName)
    {
        if (DatabaseType == DatabaseType.SQLite)
        {
            // TODO: SQLite does not support AT TIME ZONE, so we need to handle this differently.
        }
        else
        {
            scope.Database.Execute($"UPDATE {tableName} SET {columName} = {columName} AT TIME ZONE '{timezoneName}' AT TIME ZONE 'UTC'");
        }
    }
}
