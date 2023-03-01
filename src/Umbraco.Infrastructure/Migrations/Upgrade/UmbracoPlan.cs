using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade;

/// <summary>
/// Represents the Umbraco CMS migration plan.
/// </summary>
/// <seealso cref="Umbraco.Cms.Infrastructure.Migrations.MigrationPlan" />
public class UmbracoPlan : MigrationPlan
{
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
    /// Defines the plan.
    /// </summary>
    protected void DefinePlan()
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

        // To 10.0.0
        To<V_10_0_0.AddMemberPropertiesAsColumns>("{B7E0D53C-2B0E-418B-AB07-2DDE486E225F}");

        // To 10.2.0
        To<V_10_2_0.AddUserGroup2LanguageTable>("{D0B3D29D-F4D5-43E3-BA67-9D49256F3266}");
        To<V_10_2_0.AddHasAccessToAllLanguagesColumn>("{79D8217B-5920-4C0E-8E9A-3CF8FA021882}");

        // To 10.3.0
        To<V_10_3_0.AddBlockGridPartialViews>("{56833770-3B7E-4FD5-A3B6-3416A26A7A3F}");

        // To 10.4.0
        To<V_10_4_0.AddBlockGridPartialViews>("{3F5D492A-A3DB-43F9-A73E-9FEE3B180E6C}");

        // To 10.5.0 / 11.2.0
        To<V_10_5_0.AddPrimaryKeyConstrainToContentVersionCleanupDtos>("{83AF7945-DADE-4A02-9041-F3F6EBFAC319}");

        // To 11.3.0
        To<V_11_3_0.AddDomainSortOrder>("{BB3889ED-E2DE-49F2-8F71-5FD8616A2661}");
    }
}
