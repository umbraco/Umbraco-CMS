using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_7_0;

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

        // to 10.7.0
        To<MigrateTagsFromNVarcharToNText>("{EF93F398-1385-4F07-808A-D3C518984442}");

        // To 11.3.0
        To<V_11_3_0.AddDomainSortOrder>("{BB3889ED-E2DE-49F2-8F71-5FD8616A2661}");

        // To 11.4.0
        To<V_11_4_0.AlterKeyValueDataType>("{FFB6B9B0-F1A8-45E9-9CD7-25700577D1CA}");

        // to 11.4.3
        // This is here twice since it was added in 10, so if you had already upgraded you wouldn't get it
        // But we still need the first once, since if you upgraded to 10.7.0 then the "From" state would be unknown otherwise.
        // This has it's own key, we essentially consider it a separate migration.
        To<MigrateTagsFromNVarcharToNText>("{2CA0C5BB-170B-45E5-8179-E73DA4B41A46}");

        // To 12.0.0
        To<V_12_0_0.UseNvarcharInsteadOfNText>("{888A0D5D-51E4-4C7E-AA0A-01306523C7FB}");
        To<V_12_0_0.ResetCache>("{539F2F83-FBA7-4C48-81A3-75081A56BB9D}");

        // To 12.1.0
        To<V_12_1_0.TablesIndexesImprovement>("{1187192D-EDB5-4619-955D-91D48D738871}");
        To<V_12_1_0.AddOpenIddict>("{47DE85CE-1E16-42A0-8AF6-3EC3BCEF5471}");

        // And once more for 12
        To<MigrateTagsFromNVarcharToNText>("{2D4C9FBD-08B3-472D-A76C-6ED467A0CD20}");

        // To 13.0.0
        To<V_13_0_0.AddWebhooks>("{C76D9C9A-635B-4D2C-A301-05642A523E9D}");
        To<V_13_0_0.RenameEventNameColumn>("{D5139400-E507-4259-A542-C67358F7E329}");
        To<V_13_0_0.AddWebhookRequest>("{4E652F18-9A29-4656-A899-E3F39069C47E}");
        To<V_13_0_0.RenameWebhookIdToKey>("{148714C8-FE0D-4553-B034-439D91468761}");
        To<V_13_0_0.AddWebhookDatabaseLock>("{23BA95A4-FCCE-49B0-8AA1-45312B103A9B}");
        To<V_13_0_0.ChangeLogStatusCode>("{7DDCE198-9CA4-430C-8BBC-A66D80CA209F}");
        To<V_13_0_0.ChangeWebhookRequestObjectColumnToNvarcharMax>("{F74CDA0C-7AAA-48C8-94C6-C6EC3C06F599}");
    }
}
