using Microsoft.Extensions.Logging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Packaging;

public class PendingPackageMigrations
{
    private readonly ILogger<PendingPackageMigrations> _logger;
    private readonly PackageMigrationPlanCollection _packageMigrationPlans;

    public PendingPackageMigrations(
        ILogger<PendingPackageMigrations> logger,
        PackageMigrationPlanCollection packageMigrationPlans)
    {
        _logger = logger;
        _packageMigrationPlans = packageMigrationPlans;
    }

    /// <summary>
    ///     Returns what package migration names are pending
    /// </summary>
    /// <param name="keyValues">
    ///     These are the key/value pairs from the keyvalue storage of migration names and their final values
    /// </param>
    /// <returns></returns>
    public IReadOnlyList<string> GetPendingPackageMigrations(IReadOnlyDictionary<string, string?>? keyValues)
    {
        var packageMigrationPlans = _packageMigrationPlans.ToList();

        var pendingMigrations = new List<string>(packageMigrationPlans.Count);

        foreach (PackageMigrationPlan plan in packageMigrationPlans)
        {
            string? currentMigrationState = null;
            var planKeyValueKey = Constants.Conventions.Migrations.KeyValuePrefix + plan.Name;
            if (keyValues?.TryGetValue(planKeyValueKey, out var value) ?? false)
            {
                currentMigrationState = value;

                if (!plan.FinalState.InvariantEquals(value))
                {
                    // Not equal so we need to run
                    pendingMigrations.Add(plan.Name);
                }
            }
            else
            {
                // If there is nothing in the DB then we need to run
                pendingMigrations.Add(plan.Name);
            }

            _logger.LogDebug(
                "Final package migration for {PackagePlan} state is {FinalMigrationState}, database contains {DatabaseState}",
                plan.Name,
                plan.FinalState,
                currentMigrationState ?? "<null>");
        }

        return pendingMigrations;
    }
}
