using System.Net;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Umbraco.Cms.Infrastructure.HealthChecks.Checks.Data;

/// <summary>
///     Health check that verifies the Umbraco migration state recorded in the database matches the final state of the
///     <see cref="UmbracoPlan" />.
/// </summary>
[HealthCheck(
    "5C2A4E8B-3F1D-4B7A-9E6C-8D0F2A1B3C4D",
    "Database migration state",
    Description = "Checks that the latest Umbraco migration has run and is recorded in the database.",
    Group = "Data Integrity")]
public class MigrationStateCheck : HealthCheck
{
    private readonly IKeyValueService _keyValueService;
    private readonly IUmbracoVersion _umbracoVersion;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MigrationStateCheck" /> class.
    /// </summary>
    /// <param name="keyValueService">The service used to read the recorded migration state.</param>
    /// <param name="umbracoVersion">The running Umbraco version.</param>
    public MigrationStateCheck(IKeyValueService keyValueService, IUmbracoVersion umbracoVersion)
    {
        _keyValueService = keyValueService;
        _umbracoVersion = umbracoVersion;
    }

    /// <inheritdoc />
    public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync()
        => Task.FromResult<IEnumerable<HealthCheckStatus>>([CheckMigrationState()]);

    private HealthCheckStatus CheckMigrationState()
    {
        var plan = new UmbracoPlan(_umbracoVersion);
        var finalState = plan.FinalState;
        var currentState = _keyValueService.GetValue(Constants.Conventions.Migrations.UmbracoUpgradePlanKey);

        if (string.IsNullOrWhiteSpace(currentState))
        {
            return new HealthCheckStatus(
                $"<p>No migration state is recorded in the database. Expected <code>{WebUtility.HtmlEncode(finalState)}</code>.</p>")
            {
                ResultType = StatusResultType.Error,
            };
        }

        if (string.Equals(currentState, finalState, StringComparison.OrdinalIgnoreCase))
        {
            return new HealthCheckStatus(
                $"<p>The latest migration <code>{WebUtility.HtmlEncode(finalState)}</code> has run.</p>")
            {
                ResultType = StatusResultType.Success,
            };
        }

        var encodedCurrent = WebUtility.HtmlEncode(currentState);
        var encodedFinal = WebUtility.HtmlEncode(finalState);

        // A state with an outgoing transition is a known, earlier step in the plan.
        if (plan.Transitions.ContainsKey(currentState))
        {
            SemVersion? version = plan.GetVersionForState(currentState);
            var versionText = version is null ? string.Empty : $" (Umbraco {version})";
            return new HealthCheckStatus(
                $"<p>The database is at migration <code>{encodedCurrent}</code>{versionText}, but the latest migration is <code>{encodedFinal}</code>. Pending migrations have not run.</p>")
            {
                ResultType = StatusResultType.Error,
            };
        }

        return new HealthCheckStatus(
            $"<p>The recorded migration state <code>{encodedCurrent}</code> is not part of the Umbraco migration plan. Expected <code>{encodedFinal}</code>. The database may have been upgraded by a newer version of Umbraco.</p>")
        {
            ResultType = StatusResultType.Warning,
        };
    }
}
