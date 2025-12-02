using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Adds a migration to transform all system dates stored in the database from the local server timezone to UTC.
/// </summary>
public class MigrateSystemDatesToUtc : UnscopedMigrationBase
{
    private static readonly string[] UtcIdentifiers = ["Coordinated Universal Time", "UTC"];

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

        // Offsets and Windows name are lazy loaded as they are not always needed.
        // This also allows using timezones that exist in the SQL Database but not on the local server.
        (string TimeZoneName, Lazy<TimeSpan> TimeZoneOffset, Lazy<string> WindowsTimeZoneName) timeZone;

        var configuredTimeZoneName = _migrationSettings.Value.LocalServerTimeZone;
        if (configuredTimeZoneName.IsNullOrWhiteSpace() is false)
        {
            timeZone = (
                configuredTimeZoneName,
                new Lazy<TimeSpan>(() => TimeZoneInfo.FindSystemTimeZoneById(configuredTimeZoneName).BaseUtcOffset),
                new Lazy<string>(() => configuredTimeZoneName));

            _logger.LogInformation(
                "Migrating system dates to UTC using the configured timezone: {TimeZoneName}.",
                timeZone.TimeZoneName);
        }
        else
        {
            // If the local server timezone is not configured, we detect it.
            TimeZoneInfo timeZoneInfo = _timeProvider.LocalTimeZone;

            timeZone = (
                timeZoneInfo.Id,
                new Lazy<TimeSpan>(() => timeZoneInfo.BaseUtcOffset),
                new Lazy<string>(() => GetWindowsTimeZoneId(timeZoneInfo)));

            _logger.LogInformation(
                "Migrating system dates to UTC using the detected local server timezone: {TimeZoneName}.",
                timeZone.TimeZoneName);
        }

        // If the local server timezone is UTC, skip the migration.
        if (UtcIdentifiers.Contains(timeZone.TimeZoneName, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "Skipping migration {MigrationName} as the local server timezone is UTC.",
                nameof(MigrateSystemDatesToUtc));
            Context.Complete();
            return;
        }

        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();

        // Ensure we have a long command timeout as this migration can take a while on large tables within the database.
        // If the command timeout is already longer, applied via the connection string with "Connect Timeout={timeout}" we leave it as is.
        const int CommandTimeoutInSeconds = 300;
        if (scope.Database.CommandTimeout < CommandTimeoutInSeconds)
        {
            scope.Database.CommandTimeout = CommandTimeoutInSeconds;
        }

        MigrateDateColumn(scope, "cmsMember", "emailConfirmedDate", timeZone);
        MigrateDateColumn(scope, "cmsMember", "lastLoginDate", timeZone);
        MigrateDateColumn(scope, "cmsMember", "lastLockoutDate", timeZone);
        MigrateDateColumn(scope, "cmsMember", "lastPasswordChangeDate", timeZone);
        MigrateDateColumn(scope, "umbracoAccess", "createDate", timeZone);
        MigrateDateColumn(scope, "umbracoAccess", "updateDate", timeZone);
        MigrateDateColumn(scope, "umbracoAccessRule", "createDate", timeZone);
        MigrateDateColumn(scope, "umbracoAccessRule", "updateDate", timeZone);
        MigrateDateColumn(scope, "umbracoCreatedPackageSchema", "updateDate", timeZone);
        MigrateDateColumn(scope, "umbracoContentVersion", "versionDate", timeZone);
        MigrateDateColumn(scope, "umbracoContentVersionCleanupPolicy", "updated", timeZone);
        MigrateDateColumn(scope, "umbracoContentVersionCultureVariation", "date", timeZone);
        MigrateDateColumn(scope, "umbracoExternalLogin", "createDate", timeZone);
        MigrateDateColumn(scope, "umbracoExternalLoginToken", "createDate", timeZone);
        MigrateDateColumn(scope, "umbracoKeyValue", "updated", timeZone);
        MigrateDateColumn(scope, "umbracoLog", "Datestamp", timeZone);
        MigrateDateColumn(scope, "umbracoNode", "createDate", timeZone);
        MigrateDateColumn(scope, "umbracoRelation", "datetime", timeZone);
        MigrateDateColumn(scope, "umbracoServer", "registeredDate", timeZone);
        MigrateDateColumn(scope, "umbracoServer", "lastNotifiedDate", timeZone);
        MigrateDateColumn(scope, "umbracoUser", "createDate", timeZone);
        MigrateDateColumn(scope, "umbracoUser", "updateDate", timeZone);
        MigrateDateColumn(scope, "umbracoUser", "emailConfirmedDate", timeZone);
        MigrateDateColumn(scope, "umbracoUser", "lastLockoutDate", timeZone);
        MigrateDateColumn(scope, "umbracoUser", "lastPasswordChangeDate", timeZone);
        MigrateDateColumn(scope, "umbracoUser", "lastLoginDate", timeZone);
        MigrateDateColumn(scope, "umbracoUser", "invitedDate", timeZone);
        MigrateDateColumn(scope, "umbracoUserGroup", "createDate", timeZone);
        MigrateDateColumn(scope, "umbracoUserGroup", "updateDate", timeZone);

        scope.Complete();
        Context.Complete();
    }

    private static string GetWindowsTimeZoneId(TimeZoneInfo timeZone)
    {
        if (timeZone.HasIanaId is false)
        {
            return timeZone.Id;
        }

        if (TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZone.Id, out var windowsId) is false)
        {
            throw new InvalidOperationException(
                $"Could not update system dates to UTC as it was not possible to convert the detected local time zone IANA id '{timeZone.Id}' to a Windows Id necessary for updates with SQL Server. Please manually configure the 'Umbraco:CMS:SystemDateMigration:LocalServerTimeZone' app setting with a valid Windows time zone name.");
        }

        return windowsId;
    }

    private void MigrateDateColumn(
        IScope scope,
        string tableName,
        string columName,
        (string Name, Lazy<TimeSpan> BaseOffset, Lazy<string> WindowsName) timeZone)
    {
        if (DatabaseType == DatabaseType.SQLite)
        {
            MigrateDateColumnSQLite(scope, tableName, columName, timeZone.Name, timeZone.BaseOffset.Value);
        }
        else
        {
            MigrateDateColumnSqlServer(scope, tableName, columName, timeZone.WindowsName.Value);
        }
    }

    private void MigrateDateColumnSQLite(
        IScope scope,
        string tableName,
        string columName,
        string timezoneName,
        TimeSpan timezoneOffset)
    {
        // SQLite does not support AT TIME ZONE, but we can use the offset to update the dates. It won't take account of daylight saving time, but
        // given these are historical dates in expected non-production environments, that are unlikely to be necessary to be 100% accurate, this is acceptable.

        var offsetInMinutes = -timezoneOffset.TotalMinutes;
        var offsetInMinutesString = offsetInMinutes > 0
            ? $"+{offsetInMinutes}"
            : $"{offsetInMinutes}";

        Sql sql = Sql($"UPDATE {tableName} SET {columName} = DATETIME({columName}, '{offsetInMinutesString} minutes')");

        scope.Database.Execute(sql);

        _logger.LogInformation(
            "Migrated {TableName}.{ColumnName} from timezone {TimeZoneName} ({OffsetInMinutes}) to UTC.",
            tableName,
            columName,
            timezoneName,
            offsetInMinutesString);
    }

    private void MigrateDateColumnSqlServer(IScope scope, string tableName, string columName, string timeZoneName)
    {
        Sql sql = Sql($"UPDATE {tableName} SET {columName} = {columName} AT TIME ZONE '{timeZoneName}' AT TIME ZONE 'UTC'");

        scope.Database.Execute(sql);

        _logger.LogInformation(
            "Migrated {TableName}.{ColumnName} from timezone {TimeZoneName} to UTC.",
            tableName,
            columName,
            timeZoneName);
    }
}
