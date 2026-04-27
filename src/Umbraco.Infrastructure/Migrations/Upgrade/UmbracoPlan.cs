using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade;

/// <summary>
/// Represents the Umbraco CMS migration plan.
/// </summary>
/// <seealso cref="Umbraco.Cms.Infrastructure.Migrations.MigrationPlan" />
public partial class UmbracoPlan : MigrationPlan
{
    [GeneratedRegex(@"V_(\d+)_(\d+)_(\d+)")]
    internal static partial Regex MigrationStepVersionRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoPlan" /> class.
    /// </summary>
    /// <param name="umbracoVersion">The Umbraco version.</param>
    public UmbracoPlan(IUmbracoVersion umbracoVersion) // TODO (V12): Remove unused parameter
        : base(Constants.Conventions.Migrations.UmbracoUpgradePlanName)
        => DefinePlan();

    /// <inheritdoc />
    /// <remarks>
    /// This is set to the final migration state of 9.4, making that the lowest supported version to upgrade from.
    /// </remarks>
    public override string InitialState => "{DED98755-4059-41BB-ADBD-3FEAB12D1D7B}";

    /// <summary>
    /// Gets the semantic version corresponding to <see cref="InitialState"/>.
    /// </summary>
    /// <remarks>
    /// Keep in sync with <see cref="InitialState"/> whenever the lowest supported upgrade version changes.
    /// </remarks>
    private static SemVersion InitialStateVersion { get; } = new(9, 4, 0);

    /// <summary>
    /// Defines the plan.
    /// </summary>
    /// <remarks>
    /// This is virtual for testing purposes.
    /// </remarks>
    protected virtual void DefinePlan()
    {
        // Please take great care when modifying the plan!
        //
        // Creating a migration: append the migration to the main chain, using a new GUID.
        //
        // If the new migration causes a merge conflict, because someone else also added another
        // new migration, you NEED to fix the conflict by providing one default path, and paths
        // out of the conflict states, eg:
        //
        // From("state-1")
        // To<ChangeA>("state-a")
        // To<ChangeB>("state-b") // Some might already be in this state, without having applied ChangeA
        //
        // From("state-1")
        //   .Merge()
        //     .To<ChangeA>("state-a")
        //   .With()
        //     .To<ChangeB>("state-b")
        //   .As("state-2");

        From(InitialState);

        // To 17.0.0
        To<V_17_0_0.AddGuidsToAuditEntries>("{17D5F6CA-CEB8-462A-AF86-4B9C3BF91CF1}");
        To<V_17_0_0.MigrateCheckboxListDataTypesAndPropertyData>("{EB1E50B7-CD5E-4B6B-B307-36237DD2C506}");
        To<V_17_0_0.SetDateDefaultsToUtcNow>("{1847C7FF-B021-44EB-BEB0-A77A4376A6F2}");
        To<V_17_0_0.MigrateSystemDatesToUtc>("{7208B20D-6BFC-472E-9374-85EEA817B27D}");
        To<V_17_0_0.AddDistributedJobLock>("{263075BF-F18A-480D-92B4-4947D2EAB772}");
        To<V_17_0_0.AddLastSyncedTable>("26179D88-58CE-4C92-B4A4-3CBA6E7188AC");
        To<V_17_0_0.EnsureDefaultMediaFolderHasDefaultCollection>("{8B2C830A-4FFB-4433-8337-8649B0BF52C8}");
        To<V_17_0_0.InvalidateBackofficeUserAccess>("{1C38D589-26BB-4A46-9ABE-E4A0DF548A87}");

        // To 17.0.1
        To<V_17_0_1.EnsureUmbracoPropertyDataColumnCasing>("{BE5CA411-E12D-4455-A59E-F12A669E5363}");

        // To 17.1.0
        To<V_17_1_0.ChangeValidationRegExpToNvarcharMax>("{1CE2E78B-E736-45D8-97A2-CE3EF2F31BCD}");

        // To 17.2.0
        To<V_17_2_0.AddDescriptionToUserGroup>("{F1A2B3C4-D5E6-4789-ABCD-1234567890AB}");
        To<V_17_2_0.AddDocumentUrlAlias>("{A7B8C9D0-E1F2-4A5B-8C7D-9E0F1A2B3C4D}");

        // To 17.3.0
        To<V_17_3_0.IncreaseSizeOfLongRunningOperationTypeColumn>("{B2F4A1C3-8D5E-4F6A-9B7C-3E1D2A4F5B6C}");
        To<V_17_3_0.RetrustForeignKeyAndCheckConstraints>("{0638E0E0-D914-4ACA-8A4B-9551A3AAB91F}");
        To<V_17_3_0.RebuildHybridCache>("{E4A7C2D1-5F38-4B96-A1D3-8E2F6C9B0A74}");
        To<V_17_3_0.OptimizeInvariantUrlRecords>("{B8C9D0E1-F2A3-4B5C-8D7E-9F0A1B2C3D4E}");
        To<V_17_3_0.AddSortableValueToPropertyData>("{9DBDB5CD-8679-4BB0-BF83-E8D508073CE0}");
        To<V_17_3_0.PopulateSortableValueForDatePropertyData>("{6748CB56-CC16-49F0-BA91-B8ECE31BF456}");

        // To 17.4.0
        To<V_17_4_0.AddContentVersionDateIndex>("{D4E5F6A7-B8C9-4D0E-A1F2-3B4C5D6E7F80}");
        To<V_17_4_0.AddDimensionsToSvg>("{72970B86-59D8-403C-B322-FFF43F9DB199}");
        To<V_17_4_0.AddExternalMemberTables>("{D7E8F9A0-B1C2-4D3E-A5F6-7890ABCDEF12}");
        To<V_17_4_0.FixLabelDataTypeDbTypeFromConfiguration>("{3F9B6A1C-7D84-4E2B-9C15-6A2E8F3D5B47}");

        // To 18.0.0
        // TODO (V18): Enable on 18 branch
        ////To<V_18_0_0.MigrateSingleBlockList>("{74332C49-B279-4945-8943-F8F00B1F5949}");
    }

    /// <summary>
    ///     Resolves the semantic version corresponding to a migration state in this plan.
    /// </summary>
    /// <remarks>
    ///     Walks the transition chain from <see cref="MigrationPlan.InitialState"/> and extracts version
    ///     numbers from migration type namespaces (convention: <c>V_{major}_{minor}_{patch}</c>).
    /// </remarks>
    /// <param name="state">The migration state to resolve.</param>
    /// <returns>The semantic version for the state, or <c>null</c> if the state is not found.</returns>
    public SemVersion? GetVersionForState(string? state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            return null;
        }

        SemVersion? trackedVersion = InitialStateVersion;
        var current = InitialState;

        if (string.Equals(current, state, StringComparison.OrdinalIgnoreCase))
        {
            return InitialStateVersion;
        }

        while (Transitions.TryGetValue(current, out Transition? transition) && transition is not null)
        {
            Match match = MigrationStepVersionRegex().Match(transition.MigrationType.Namespace ?? string.Empty);
            if (match.Success)
            {
                trackedVersion = new SemVersion(
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value));
            }

            if (string.Equals(transition.TargetState, state, StringComparison.OrdinalIgnoreCase))
            {
                return trackedVersion;
            }

            current = transition.TargetState;
        }

        return null;
    }
}
