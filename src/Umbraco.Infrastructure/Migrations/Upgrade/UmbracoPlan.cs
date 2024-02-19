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

        // To 13.0.0
        To<V_13_0_0.AddWebhooks>("{C76D9C9A-635B-4D2C-A301-05642A523E9D}");
        To<V_13_0_0.RenameEventNameColumn>("{D5139400-E507-4259-A542-C67358F7E329}");
        To<V_13_0_0.AddWebhookRequest>("{4E652F18-9A29-4656-A899-E3F39069C47E}");
        To<V_13_0_0.RenameWebhookIdToKey>("{148714C8-FE0D-4553-B034-439D91468761}");
        To<V_13_0_0.AddWebhookDatabaseLock>("{23BA95A4-FCCE-49B0-8AA1-45312B103A9B}");
        To<V_13_0_0.ChangeLogStatusCode>("{7DDCE198-9CA4-430C-8BBC-A66D80CA209F}");
        To<V_13_0_0.ChangeWebhookRequestObjectColumnToNvarcharMax>("{F74CDA0C-7AAA-48C8-94C6-C6EC3C06F599}");
        To<V_13_0_0.ChangeWebhookUrlColumnsToNvarcharMax>("{21C42760-5109-4C03-AB4F-7EA53577D1F5}");
        To<V_13_0_0.AddExceptionOccured>("{6158F3A3-4902-4201-835E-1ED7F810B2D8}");

        // To 14.0.0
        To<V_14_0_0.AddPropertyEditorUiAliasColumn>("{419827A0-4FCE-464B-A8F3-247C6092AF55}");
        To<V_14_0_0.MigrateDataTypeConfigurations>("{5F15A1CC-353D-4889-8C7E-F303B4766196}");
        To<V_14_0_0.AddGuidsToUserGroups>("{69E12556-D9B3-493A-8E8A-65EC89FB658D}");
        To<V_14_0_0.AddUserGroup2PermisionTable>("{F2B16CD4-F181-4BEE-81C9-11CF384E6025}");
        To<V_14_0_0.AddGuidsToUsers>("{A8E01644-9F2E-4988-8341-587EF5B7EA69}");
        To<V_14_0_0.UpdateDefaultGuidsOfCreatedPackages>("{E073DBC0-9E8E-4C92-8210-9CB18364F46E}");
        To<V_14_0_0.RenameTechnologyLeakingPropertyEditorAliases>("{80D282A4-5497-47FF-991F-BC0BCE603121}");
    }
}
