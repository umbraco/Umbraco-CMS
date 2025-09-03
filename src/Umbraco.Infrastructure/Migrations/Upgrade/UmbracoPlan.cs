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
        To<V_13_3_0.AlignUpgradedDatabase>("{985AF2BA-69D3-4DBA-95E0-AD3FA7459FA7}");
        To<V_13_5_0.ChangeRedirectUrlToNvarcharMax>("{CC47C751-A81B-489A-A2BC-0240245DB687}");

        // To 14.0.0
        To<NoopMigration>("{419827A0-4FCE-464B-A8F3-247C6092AF55}");
        To<NoopMigration>("{69E12556-D9B3-493A-8E8A-65EC89FB658D}");
        To<NoopMigration>("{F2B16CD4-F181-4BEE-81C9-11CF384E6025}");
        To<NoopMigration>("{A8E01644-9F2E-4988-8341-587EF5B7EA69}");
        To<V_14_0_0.UpdateDefaultGuidsOfCreatedPackages>("{E073DBC0-9E8E-4C92-8210-9CB18364F46E}");
        To<V_14_0_0.RenameTechnologyLeakingPropertyEditorAliases>("{80D282A4-5497-47FF-991F-BC0BCE603121}");
        To<V_14_0_0.MigrateSchduledPublishesToUtc>("{96525697-E9DC-4198-B136-25AD033442B8}");
        To<NoopMigration>("{7FC5AC9B-6F56-415B-913E-4A900629B853}");
        To<V_14_0_0.MigrateDataTypeConfigurations>("{1539A010-2EB5-4163-8518-4AE2AA98AFC6}");
        To<NoopMigration>("{C567DE81-DF92-4B99-BEA8-CD34EF99DA5D}");
        To<V_14_0_0.DeleteMacroTables>("{0D82C836-96DD-480D-A924-7964E458BD34}");
        To<V_14_0_0.MoveDocumentBlueprintsToFolders>("{1A0FBC8A-6FC6-456C-805C-B94816B2E570}");
        To<NoopMigration>("{302DE171-6D83-4B6B-B3C0-AC8808A16CA1}");
        To<V_14_0_0.MigrateUserGroup2PermissionPermissionColumnType>("{8184E61D-ECBA-4AAA-B61B-D7A82EB82EB7}");
        To<V_14_0_0.MigrateNotificationCharsToStrings>("{E261BF01-2C7F-4544-BAE7-49D545B21D68}");
        To<V_14_0_0.AddEditorUiToDataType>("{5A2EF07D-37B4-49D5-8E9B-3ED01877263B}");
        // we need to re-run this migration, as it was flawed for V14 RC3 (the migration can run twice without any issues)
        To<V_14_0_0.AddEditorUiToDataType>("{6FB5CA9E-C823-473B-A14C-FE760D75943C}");
        To<V_14_0_0.CleanUpDataTypeConfigurations>("{827360CA-0855-42A5-8F86-A51F168CB559}");

        // To 14.1.0
        To<V_14_1_0.MigrateRichTextConfiguration>("{FEF2DAF4-5408-4636-BB0E-B8798DF8F095}");
        To<V_14_1_0.MigrateOldRichTextSeedConfiguration>("{A385C5DF-48DC-46B4-A742-D5BB846483BC}");

        // To 14.2.0
        To<V_14_2_0.AddMissingDateTimeConfiguration>("{20ED404C-6FF9-4F91-8AC9-2B298E0002EB}");

        // To 14.3.0
        To<V_13_5_0.ChangeRedirectUrlToNvarcharMax>("{EEF792FC-318C-4921-9859-51EBF07A53A3}"); // Execute again, to ensure all that migrated to 14.0.0 without 13.5 will have this

        // To 15.0.0
        To<V_15_0_0.AddUserClientId>("{7F4F31D8-DD71-4F0D-93FC-2690A924D84B}");
        To<NoopMigration>("{1A8835EF-F8AB-4472-B4D8-D75B7C164022}");
        To<V_15_0_0.RebuildDocumentUrls>("{3FE0FA2D-CF4F-4892-BA8D-E97D06E028DC}");
        To<V_15_0_0.ConvertBlockListEditorProperties>("{6C04B137-0097-4938-8C6A-276DF1A0ECA8}");
        To<V_15_0_0.ConvertBlockGridEditorProperties>("{9D3CE7D4-4884-41D4-98E8-302EB6CB0CF6}");
        To<V_15_0_0.ConvertRichTextEditorProperties>("{37875E80-5CDD-42FF-A21A-7D4E3E23E0ED}");
        To<V_15_0_0.ConvertLocalLinks>("{42E44F9E-7262-4269-922D-7310CB48E724}");

        // To 15.1.0
        To<V_15_1_0.RebuildCacheMigration>("{7B51B4DE-5574-4484-993E-05D12D9ED703}");
        To<V_15_1_0.FixConvertLocalLinks>("{F3D3EF46-1B1F-47DB-B437-7D573EEDEB98}");

        // To 15.3.0
        To<V_15_3_0.AddNameAndDescriptionToWebhooks>("{7B11F01E-EE33-4B0B-81A1-F78F834CA45B}");

        // To 15.4.0
        To<V_15_4_0.UpdateDocumentUrlToPersistMultipleSegmentsPerDocument>("{A9E72794-4036-4563-B543-1717C73B8879}");
        To<V_15_4_0.AddRelationTypeForMembers>("{33D62294-D0DE-4A86-A830-991EB36B96DA}");

        // To 16.0.0
        To<V_16_0_0.MigrateTinyMceToTiptap>("{C6681435-584F-4BC8-BB8D-BC853966AF0B}");
        To<V_16_0_0.AddDocumentPropertyPermissions>("{D1568C33-A697-455F-8D16-48060CB954A1}");

        // To 16.2.0
        To<V_16_2_0.AddLongRunningOperations>("{741C22CF-5FB8-4343-BF79-B97A58C2CCBA}");
        To<NoopMigration>("{BE11D4D3-3A1F-4598-90D4-B548BD188C48}"); // Originally was V_16_2_0.AddDocumentUrlLock, now moved to a pre-migration.

        // To 16.3.0
        To<V_16_3_0.AddRichTextEditorCapabilities>("{A917FCBC-C378-4A08-A36C-220C581A6581}");
        To<V_16_3_0.MigrateMediaTypeLabelProperties>("{FB7073AF-DFAF-4AC1-800D-91F9BD5B5238}");
    }
}
