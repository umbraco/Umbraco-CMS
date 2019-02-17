using System;
using System.Configuration;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.Migrations.Upgrade.V_7_12_0;
using Umbraco.Core.Migrations.Upgrade.V_7_14_0;
using Umbraco.Core.Migrations.Upgrade.V_8_0_0;

namespace Umbraco.Core.Migrations.Upgrade
{
    /// <summary>
    /// Represents Umbraco's migration plan.
    /// </summary>
    public class UmbracoPlan : MigrationPlan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoPlan"/> class.
        /// </summary>
        public UmbracoPlan()
            : base(Constants.System.UmbracoUpgradePlanName)
        {
            DefinePlan();
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>The default initial state in plans is string.Empty.</para>
        /// <para>When upgrading from version 7, we want to use specific initial states
        /// that are e.g. "{init-7.9.3}", "{init-7.11.1}", etc. so we can chain the proper
        /// migrations.</para>
        /// <para>This is also where we detect the current version, and reject invalid
        /// upgrades (from a tool old version, or going back in time, etc).</para>
        /// </remarks>
        public override string InitialState
        {
            get
            {
                // no state in database yet - assume we have something in web.config that makes some sense
                if (!SemVersion.TryParse(ConfigurationManager.AppSettings[Constants.AppSettings.ConfigurationStatus], out var currentVersion))
                    throw new InvalidOperationException($"Could not get current version from web.config {Constants.AppSettings.ConfigurationStatus} appSetting.");

                // we currently support upgrading from 7.10.0 and later
                if (currentVersion < new SemVersion(7, 10))
                    throw new InvalidOperationException($"Version {currentVersion} cannot be migrated to {UmbracoVersion.SemanticVersion}.");

                // cannot go back in time
                if (currentVersion > UmbracoVersion.SemanticVersion)
                    throw new InvalidOperationException($"Version {currentVersion} cannot be downgraded to {UmbracoVersion.SemanticVersion}.");

                // upgrading from version 7 => initial state is eg "{init-7.10.0}"
                // anything else is not supported - ie if 8 and above, we should have an initial state already
                if (currentVersion.Major != 7)
                    throw new InvalidOperationException($"Version {currentVersion} is not supported by the migration plan.");

                return "{init-" + currentVersion + "}";
            }
        }

        // define the plan
        protected void DefinePlan()
        {
            // MODIFYING THE PLAN
            //
            // Please take great care when modifying the plan!
            //
            // * Creating a migration for version 8:
            //     Append the migration to the main chain, using a new guid, before the "//FINAL" comment
            //
            //     If the new migration causes a merge conflict, because someone else also added another
            //     new migration, you NEED to fix the conflict by providing one default path, and paths
            //     out of the conflict states (see example below).
            //
            // * Porting from version 7:
            //     Append the ported migration to the main chain, using a new guid (same as above).
            //     Create a new special chain from the {init-...} state to the main chain (see example
            //     below).


            // plan starts at 7.10.0 (anything before 7.10.0 is not supported)
            // upgrades from 7 to 8, and then takes care of all eventual upgrades
            //
            From("{init-7.10.0}");
            To<AddLockObjects>("{7C447271-CA3F-4A6A-A913-5D77015655CB}");
            To<AddContentNuTable>("{CBFF58A2-7B50-4F75-8E98-249920DB0F37}");
            To<RefactorXmlColumns>("{3D18920C-E84D-405C-A06A-B7CEE52FE5DD}");
            To<VariantsMigration>("{FB0A5429-587E-4BD0-8A67-20F0E7E62FF7}");
            To<DropMigrationsTable>("{F0C42457-6A3B-4912-A7EA-F27ED85A2092}");
            To<DataTypeMigration>("{8640C9E4-A1C0-4C59-99BB-609B4E604981}");
            To<TagsMigration>("{DD1B99AF-8106-4E00-BAC7-A43003EA07F8}");
            To<SuperZero>("{9DF05B77-11D1-475C-A00A-B656AF7E0908}");
            To<PropertyEditorsMigration>("{6FE3EF34-44A0-4992-B379-B40BC4EF1C4D}");
            To<LanguageColumns>("{7F59355A-0EC9-4438-8157-EB517E6D2727}");
            ToWithReplace<AddVariationTables2, AddVariationTables1A>("{941B2ABA-2D06-4E04-81F5-74224F1DB037}", "{76DF5CD7-A884-41A5-8DC6-7860D95B1DF5}"); // kill AddVariationTable1
            To<RefactorMacroColumns>("{A7540C58-171D-462A-91C5-7A9AA5CB8BFD}");

            Merge().To<UserForeignKeys>("{3E44F712-E2E3-473A-AE49-5D7F8E67CE3F}") // shannon added that one
            .With().To<AddTypedLabels>("{65D6B71C-BDD5-4A2E-8D35-8896325E9151}") // stephan added that one
            .As("{4CACE351-C6B9-4F0C-A6BA-85A02BBD39E4}");

            To<ContentVariationMigration>("{1350617A-4930-4D61-852F-E3AA9E692173}");
            To<UpdateUmbracoConsent>("{39E5B1F7-A50B-437E-B768-1723AEC45B65}"); // from 7.12.0

            Merge()
                .To<AddRelationTypeForMediaFolderOnDelete>("{0541A62B-EF87-4CA2-8225-F0EB98ECCC9F}") // from 7.12.0
                .To<IncreaseLanguageIsoCodeColumnLength>("{EB34B5DC-BB87-4005-985E-D983EA496C38}") // from 7.12.0
                .To<RenameTrueFalseField>("{517CE9EA-36D7-472A-BF4B-A0D6FB1B8F89}") // from 7.12.0
                .To<SetDefaultTagsStorageType>("{BBD99901-1545-40E4-8A5A-D7A675C7D2F2}") // from 7.12.0
            .With()
                .To<FallbackLanguage>("{CF51B39B-9B9A-4740-BB7C-EAF606A7BFBF}") // andy added that one
            .As("{8B14CEBD-EE47-4AAD-A841-93551D917F11}");

            ToWithReplace<UpdateDefaultMandatoryLanguage>("{2C87AA47-D1BC-4ECB-8A73-2D8D1046C27F}", "{5F4597F4-A4E0-4AFE-90B5-6D2F896830EB}"); // merge
            ToWithReplace<RefactorVariantsModel>("{B19BF0F2-E1C6-4AEB-A146-BC559D97A2C6}", "{290C18EE-B3DE-4769-84F1-1F467F3F76DA}"); // merge
            To<DropTaskTables>("{6A2C7C1B-A9DB-4EA9-B6AB-78E7D5B722A7}");
            To<FixLockTablePrimaryKey>("{77874C77-93E5-4488-A404-A630907CEEF0}");
            To<AddLogTableColumns>("{8804D8E8-FE62-4E3A-B8A2-C047C2118C38}");
            To<DropPreValueTable>("{23275462-446E-44C7-8C2C-3B8C1127B07D}");
            To<DropDownPropertyEditorsMigration>("{6B251841-3069-4AD5-8AE9-861F9523E8DA}");
            To<TagsMigrationFix>("{EE429F1B-9B26-43CA-89F8-A86017C809A3}");
            To<DropTemplateDesignColumn>("{08919C4B-B431-449C-90EC-2B8445B5C6B1}");
            To<TablesForScheduledPublishing>("{7EB0254C-CB8B-4C75-B15B-D48C55B449EB}");
            To<DropTaskTables>("{648A2D5F-7467-48F8-B309-E99CEEE00E2A}"); // fixed version
            To<MakeTagsVariant>("{C39BF2A7-1454-4047-BBFE-89E40F66ED63}");
            To<MakeRedirectUrlVariant>("{64EBCE53-E1F0-463A-B40B-E98EFCCA8AE2}");
            To<AddContentTypeIsElementColumn>("{0009109C-A0B8-4F3F-8FEB-C137BBDDA268}");
            To<UpdateMemberGroupPickerData>("{8A027815-D5CD-4872-8B88-9A51AB5986A6}"); // from 7.14.0
            To<ConvertRelatedLinksToMultiUrlPicker>("{ED28B66A-E248-4D94-8CDB-9BDF574023F0}");
            To<UpdatePickerIntegerValuesToUdi>("{38C809D5-6C34-426B-9BEA-EFD39162595C}");
            To<RenameUmbracoDomainsTable>("{6017F044-8E70-4E10-B2A3-336949692ADD}");
            To<AddUserLoginDtoDateIndex>("{98339BEF-E4B2-48A8-B9D1-D173DC842BBE}");

            Merge()
                .To<DropXmlTables>("{CDBEDEE4-9496-4903-9CF2-4104E00FF960}")
            .With()
                .To<RadioAndCheckboxAndDropdownPropertyEditorsMigration>("{940FD19A-00A8-4D5C-B8FF-939143585726}")
            .As("{0576E786-5C30-4000-B969-302B61E90CA3}");

            To<RenameLabelAndRichTextPropertyEditorAliases>("{E0CBE54D-A84F-4A8F-9B13-900945FD7ED9}");

            //FINAL




            // and then, need to support upgrading from more recent 7.x
            //
            From("{init-7.10.1}").To("{init-7.10.0}"); // same as 7.10.0
            From("{init-7.10.2}").To("{init-7.10.0}"); // same as 7.10.0
            From("{init-7.10.3}").To("{init-7.10.0}"); // same as 7.10.0
            From("{init-7.10.4}").To("{init-7.10.0}"); // same as 7.10.0
            From("{init-7.10.5}").To("{init-7.10.0}"); // same as 7.10.0
            From("{init-7.11.0}").To("{init-7.10.0}"); // same as 7.10.0
            From("{init-7.11.1}").To("{init-7.10.0}"); // same as 7.10.0
            From("{init-7.11.2}").To("{init-7.10.0}"); // same as 7.10.0

            // 7.12.0 has migrations, define a custom chain which copies the chain
            // going from {init-7.10.0} to former final (1350617A) , and then goes straight to
            // main chain, skipping the migrations
            //
            From("{init-7.12.0}");
            // clone start / clone stop / target
            ToWithClone("{init-7.10.0}", "{1350617A-4930-4D61-852F-E3AA9E692173}", "{BBD99901-1545-40E4-8A5A-D7A675C7D2F2}");

            From("{init-7.12.1}").To("{init-7.10.0}"); // same as 7.12.0
            From("{init-7.12.2}").To("{init-7.10.0}"); // same as 7.12.0
            From("{init-7.12.3}").To("{init-7.10.0}"); // same as 7.12.0
            From("{init-7.12.4}").To("{init-7.10.0}"); // same as 7.12.0
            From("{init-7.13.0}").To("{init-7.10.0}"); // same as 7.12.0
            From("{init-7.13.1}").To("{init-7.10.0}"); // same as 7.12.0

            // 7.14.0 has migrations, handle it...
            //  clone going from 7.10 to 1350617A (the last one before we started to merge 7.12 migrations), then
            //  clone going from CF51B39B (after 7.12 migrations) to 0009109C (the last one before we started to merge 7.12 migrations),
            //  ending in 8A027815 (after 7.14 migrations)
            From("{init-7.14.0}")
                .ToWithClone("{init-7.10.0}", "{1350617A-4930-4D61-852F-E3AA9E692173}", "{9109B8AF-6B34-46EE-9484-7434196D0C79}")
                .ToWithClone("{CF51B39B-9B9A-4740-BB7C-EAF606A7BFBF}", "{0009109C-A0B8-4F3F-8FEB-C137BBDDA268}", "{8A027815-D5CD-4872-8B88-9A51AB5986A6}");
        }
    }
}
