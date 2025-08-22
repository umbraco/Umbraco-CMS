using NPoco;
using Umbraco.Cms.Infrastructure.Scoping;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Adds a migration to transform all system dates stored in the database from the local server timezone to UTC.
/// </summary>
public class MigrateSystemDatesToUtc : UnscopedMigrationBase
{
    private readonly IScopeProvider _scopeProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IOptions<SystemDateMigrationSettings> _migrationSettings;
    private readonly ILogger<MigrateSystemDatesToUtc> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateSystemDatesToUtc"/> class.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="scopeProvider"></param>
    /// <param name="timeProvider"></param>
    /// <param name="migrationSettings"></param>
    /// <param name="logger"></param>
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

    /// <inheritdoc/>
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
            timeZoneName = _timeProvider.LocalTimeZone.Id;
            _logger.LogInformation("Migrating system dates to UTC using the detected local server timezone: {TimeZoneName}.", timeZoneName);
        }
        else
        {
            _logger.LogInformation("Migrating system dates to UTC using the configured local server timezone: {TimeZoneName}.", timeZoneName);
        }

        // If the local server timezone is UTC, skip the migration.
        if (string.Equals(timeZoneName, "Coordinated Universal Time", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Skipping migration {MigrationName} as the local server timezone is UTC.", nameof(MigrateSystemDatesToUtc));
            Context.Complete();
            return;
        }

        TimeSpan timeZoneOffset = GetTimezoneOffset(timeZoneName);

        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();

        MigrateDateColumn(scope, "cmsMember", "emailConfirmedDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "cmsMember", "lastLoginDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "cmsMember", "lastLockoutDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "cmsMember", "lastPasswordChangeDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoAccess", "createDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoAccess", "updateDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoAccessRule", "createDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoAccessRule", "updateDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoCreatedPackageSchema", "updateDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoContentVersion", "versionDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoContentVersionCleanupPolicy", "updated", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoContentVersionCultureVariation", "date", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoExternalLogin", "createDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoExternalLoginToken", "createDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoKeyValue", "updated", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoLog", "Datestamp", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoNode", "createDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoRelation", "datetime", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoServer", "registeredDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoServer", "lastNotifiedDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoUser", "createDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoUser", "updateDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoUser", "emailConfirmedDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoUser", "lastLockoutDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoUser", "lastPasswordChangeDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoUser", "lastLoginDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoUser", "invitedDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoUserGroup", "createDate", timeZoneName, timeZoneOffset);
        MigrateDateColumn(scope, "umbracoUserGroup", "updateDate", timeZoneName, timeZoneOffset);

        scope.Complete();
        Context.Complete();
    }

    private static TimeSpan GetTimezoneOffset(string timeZoneName)

        // We know the provided timezone name exists, as it's either detected or configured (and configuration has been validated).
        => TimeZoneInfo.FindSystemTimeZoneById(timeZoneName).BaseUtcOffset;

    private void MigrateDateColumn(IScope scope, string tableName, string columName, string timezoneName, TimeSpan timeZoneOffset)
    {
        var offsetInMinutes = -timeZoneOffset.TotalMinutes;
        var offSetInMinutesString = offsetInMinutes > 0
            ? $"+{offsetInMinutes}"
            : $"{offsetInMinutes}";

        Sql sql;
        if (DatabaseType == DatabaseType.SQLite)
        {
            // SQLite does not support AT TIME ZONE, but we can use the offset to update the dates. It won't take account of daylight saving time, but
            // given these are historical dates in expected non-production environments, that are unlikely to be necessary to be 100% accurate, this is acceptable.
            sql = Sql($"UPDATE {tableName} SET {columName} = DATETIME({columName}, '{offSetInMinutesString} minutes')");
        }
        else
        {
            sql = Sql($"UPDATE {tableName} SET {columName} = {columName} AT TIME ZONE '{timezoneName}' AT TIME ZONE 'UTC'");
        }

        scope.Database.Execute(sql);

        _logger.LogInformation(
            "Migrated {TableName}.{ColumnName} from local server timezone of {TimeZoneName} ({OffSetInMinutes} minutes) to UTC.",
            tableName,
            columName,
            timezoneName,
            offSetInMinutesString);
    }
}
