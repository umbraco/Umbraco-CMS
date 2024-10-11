using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade;

/// <summary>
/// Represents the Umbraco CMS pre-migration plan. - Migrations that always runs unattended before the main migration plan.
/// </summary>
/// <seealso cref="Umbraco.Cms.Infrastructure.Migrations.MigrationPlan" />
public class UmbracoPremigrationPlan : MigrationPlan
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoPlan" /> class.
    /// </summary>
    public UmbracoPremigrationPlan()
        : base(Constants.Conventions.Migrations.UmbracoUpgradePlanPremigrationsName)
        => DefinePlan();

    /// <inheritdoc />
    /// <remarks>
    /// This is set to the final migration state of 13.0, making that the lowest supported version to upgrade from.
    /// </remarks>
    public override string InitialState => string.Empty;

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

        // To 14.0.0
        To<V_14_0_0.UpdateToOpenIddictV5>("{76FBF80E-37E6-462E-ADC1-25668F56151D}");
        To<V_14_0_0.AddGuidsToUserGroups>("{37CF4AC3-8489-44BC-A7E8-64908FEEC656}");
        To<V_14_0_0.AddUserGroup2PermisionTable>("{7BCB5352-B2ED-4D4B-B27D-ECDED930B50A}");
        To<V_14_0_0.AddGuidsToUsers>("{3E69BF9B-BEAB-41B1-BB11-15383CCA1C7F}");
        To<V_14_0_0.MigrateCharPermissionsToStrings>("{F12C609B-86B9-4386-AFA4-78E02857247C}");
    }
}
