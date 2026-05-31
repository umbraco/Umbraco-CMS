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

        // To 17.0.0
        To<V_17_0_0.UpdateToOpenIddictV7>("{D54EE168-C19D-48D8-9006-C7E719AD61FE}");
        // The lock and table are required to access caches.
        // When logging in, we save the user to the cache so these need to have run.
        To<V_17_0_0.AddCacheVersionDatabaseLock>("{1DC39DC7-A88A-4912-8E60-4FD36246E8D1}");
        To<V_17_0_0.AddRepositoryCacheVersionTable>("{A1B3F5D6-4C8B-4E7A-9F8C-1D2B3E4F5A6B}");
        To<V_17_0_0.AddLastSyncedTable>("{72894BCA-9FAD-4CC3-B0D0-D6CDA2FFA636}");

        // To 18.0.0
        To<V_18_0_0.AddElements>("{E51033DE-B4F9-45F3-87B3-0E774B2939C2}");
        To<V_18_0_0.AddAllowedInLibraryToContentType>("{31C0D92A-49DD-47EC-B2A7-932A58FF224E}");
    }
}
