using System;
using System.Configuration;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Upgrade.V_7_10_0;
using Umbraco.Core.Migrations.Upgrade.V_7_5_0;
using Umbraco.Core.Migrations.Upgrade.V_7_5_5;
using Umbraco.Core.Migrations.Upgrade.V_7_6_0;
using Umbraco.Core.Migrations.Upgrade.V_7_7_0;
using Umbraco.Core.Migrations.Upgrade.V_7_8_0;
using Umbraco.Core.Migrations.Upgrade.V_7_9_0;
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
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoPlan"/> class.
        /// </summary>
        public UmbracoPlan(IMigrationBuilder migrationBuilder, ILogger logger)
            : base(Constants.System.UmbracoUpgradePlanName, migrationBuilder, logger)
        { }

        /// <inheritdoc />
        /// <remarks>
        /// <para>The default initial state in plans is string.Empty.</para>
        /// <para>When upgrading from version 7, we want to use specific initial states
        /// that are e.g. "{orig-7.9.3}", "{orig-7.11.1}", etc. so we can chain the proper
        /// migrations.</para>
        /// <para>This is also where we detect the current version, and reject invalid
        /// upgrades (from a tool old version, or going back in time, etc).</para>
        /// </remarks>
        public override string InitialState
        {
            get
            {
                // no state in database yet - assume we have something in web.config that makes some sense
                if (!SemVersion.TryParse(ConfigurationManager.AppSettings["umbracoConfigurationStatus"], out var currentVersion))
                    throw new InvalidOperationException("Could not get current version from web.config umbracoConfigurationStatus appSetting.");

                // must be at least 7.? - fixme adjust when releasing
                if (currentVersion < new SemVersion(7, 10))
                    throw new InvalidOperationException($"Version {currentVersion} cannot be upgraded to {UmbracoVersion.SemanticVersion}.");

                // cannot go back in time
                if (currentVersion > UmbracoVersion.SemanticVersion)
                    throw new InvalidOperationException($"Version {currentVersion} cannot be downgraded to {UmbracoVersion.SemanticVersion}.");

                switch (currentVersion.Major)
                {
                    case 7:
                        // upgrading from version 7
                        return "{orig-" + currentVersion + "}";
                    case 8: // fixme remove when releasing
                        // upgrading from version 8
                        // should never happen, this is very temp and for my own website - zpqrtbnk
                        return "{04F54303-3055-4700-8F76-35A37F232FF5}"; // right before the variants migration
                    default:
                        throw new InvalidOperationException($"Version {currentVersion} is not supported by the migration plan.");
                }

            }
        }

        /// <inheritdoc />
        protected override void DefinePlan()
        {
            // NOTE: MODIFYING THE PLAN
            //
            // Please take great care when modifying the plan!
            //
            // * Creating a migration for version 8:
            //     Append the migration to the main version 8 chain, using a new guid.
            //     Update the final state (see end of file) to that guid.
            //     Append the migration to version 7 upgrade chains.
            // * Porting a migration from version 7:
            //     Append the migration to the main version 8 chain, using a new guid.
            //     Update the final state (see end of file) to that guid.
            //     Update all init-7.x.y chains.


            // UPGRADE FROM 7, OLDEST
            //
            // When upgrading from version 7, the state is automatically set to {init-7.x.y} where
            // 7.x.y is the version. We need to define a chain starting at that state and taking
            // us to version 8. And we need such a chain for each 7.x.y version that can be upgraded
            // to version 8, bearing in mind that new releases of version 7 will probably be
            // created *after* the first released of version 8.
            //
            // fixme adjust when releasing the first public (alpha?) version

            // we don't support upgrading from versions older than 7.?
            // and then we only need to run v8 migrations
            //
            From("{init-7.10.0}");
            Chain<V_8_0_0.AddLockObjects>("{7C447271-CA3F-4A6A-A913-5D77015655CB}");
            Chain<AddContentNuTable>("{CBFF58A2-7B50-4F75-8E98-249920DB0F37}");
            Chain<RefactorXmlColumns>("{3D18920C-E84D-405C-A06A-B7CEE52FE5DD}");

            Chain<VariantsMigration>("{FB0A5429-587E-4BD0-8A67-20F0E7E62FF7}");
            Chain<DropMigrationsTable>("{F0C42457-6A3B-4912-A7EA-F27ED85A2092}");
            Chain<DataTypeMigration>("{8640C9E4-A1C0-4C59-99BB-609B4E604981}");
            Chain<TagsMigration>("{DD1B99AF-8106-4E00-BAC7-A43003EA07F8}");
            Chain<SuperZero>("{9DF05B77-11D1-475C-A00A-B656AF7E0908}");
            Chain<PropertyEditorsMigration>("{6FE3EF34-44A0-4992-B379-B40BC4EF1C4D}");
            Chain<LanguageColumns>("{7F59355A-0EC9-4438-8157-EB517E6D2727}");
            Chain<AddVariationTables1A>("{66B6821A-0DE3-4DF8-A6A4-65ABD211EDDE}");
            Chain<AddVariationTables2>("{49506BAE-CEBB-4431-A1A6-24AD6EBBBC57}");
            Chain<RefactorMacroColumns>("{083A9894-903D-41B7-B6B3-9EAF2D4CCED0}");
            Chain<UserForeignKeys>("{42097524-0F8C-482C-BD79-AC7407D8A028}");
            Chain<AddTypedLabels>("{3608CD41-792A-4E9A-A97D-42A5E797EE31}");
            Chain<ContentVariationMigration>("{608A02B8-B1A1-4C24-8955-0B95DB1F567E}");

            // must chain to v8 final state (see at end of file)
            Chain("{1350617A-4930-4D61-852F-E3AA9E692173}");


            // UPGRADE FROM 7, MORE RECENT
            //
            // handle more recent versions - not yet

            // for more recent versions...
            // 7.10.x = same as 7.10.0?
            //From("{init-7.10.1}").Chain("{init-7.10.0}");


            // VERSION 8 PLAN
            //
            // this is the master Umbraco migration plan, starting from the very first version 8
            // release, which was a pre-pre-alpha, years ago. It contains migrations created
            // for version 8, along with migrations ported from version 7 as version 7 evolves.
            // It is therefore *normal* that some pure version 8 migrations are mixed with
            // migrations merged from version 7.
            //
            // new migrations should always be *appended* to the *end* of the chain.

            // 8.0.0
            From("{init-origin}"); // "origin" was 7.4.something
            Chain<V_8_0_0.AddLockTable>("{98347B5E-65BF-4DD7-BB43-A09CB7AF4FCA}");
            Chain<V_8_0_0.AddLockObjects>("{1E8165C4-942D-40DC-AC76-C5FF8831E400}");
            Chain<AddContentNuTable>("{39E15568-7AAD-4D54-81D0-758CCFC529F8}");
            Chain<RefactorXmlColumns>("{55C3F97D-BDA7-4FB1-A743-B0456B56EAA3}");

            // merging from 7.5.0
            Chain<RemoveStylesheetDataAndTablesAgain>("{287F9E39-F673-42F7-908C-21659AB13B13}");
            Chain<V_7_5_0.UpdateUniqueIndexOnPropertyData>("{2D08588A-AD90-479C-9F6E-A99B60BA7226}");
            Chain<AddRedirectUrlTable>("{2D917FF8-AC81-4C00-A407-1F4B1DF6089C}");

            // merging from 7.5.5
            Chain<UpdateAllowedMediaTypesAtRoot>("{44484C32-EEB3-4A12-B1CB-11E02CE22AB2}");

            // merging from 7.6.0
            Chain<AddIndexesToUmbracoRelationTables>("{3586E4E9-2922-49EB-8E2A-A530CE6DBDE0}");
            Chain<AddIndexToCmsMemberLoginName>("{D4A5674F-654D-4CC7-85E5-CFDBC533A318}");
            Chain<AddIndexToUmbracoNodePath>("{7F828EDD-6622-4A8D-AD80-EEAF46C11680}");
            Chain<AddMacroUniqueIdColumn>("{F30AC223-D277-4D1F-B2AB-F0F0D3546CE1}");
            Chain<AddRelationTypeUniqueIdColumn>("{7C27E310-CF48-4637-A22E-8D87355161C1}");
            Chain<NormalizeTemplateGuids>("{7D2ABA16-EE48-4569-8827-E81370FC4871}");
            Chain<ReduceLoginNameColumnsSize>("{02879EDF-13A8-43AF-87A5-DD85723D0016}");
            Chain<V_7_6_0.UpdateUniqueIndexOnPropertyData>("{5496C6CC-3AE0-4789-AF49-5BB4E28FA424}");
            Chain<RemoveUmbracoDeployTables>("{8995332B-085E-4C0C-849E-9A77E79F4293}");

            // merging from 7.7.0
            Chain<AddIndexToDictionaryKeyColumn>("{74319856-7681-46B1-AA0D-F7E896FBE6A1}");
            Chain<AddUserGroupTables>("{0427B0A2-994A-4AB4-BFF3-31B20614F6C9}");
            Chain<AddUserStartNodeTable>("{F0D6F782-E432-46DE-A3A7-2AF06DB8853B}");
            Chain<EnsureContentTemplatePermissions>("{AEB2BA2B-71E4-4B1B-AB6C-CEFB7F06FEEB}");
            Chain<ReduceDictionaryKeyColumnsSize>("{B5A6C799-B91E-496F-A1FE-7B4FE98BF6AB}");
            Chain<UpdateUserTables>("{04F54303-3055-4700-8F76-35A37F232FF5}");

            // 8.0.0
            Chain<VariantsMigration>("{6550C7E8-77B7-4DE3-9B58-E31C81CB9504}");
            Chain<DropMigrationsTable>("{E3388F73-89FA-45FE-A539-C7FACC8D63DD}");
            Chain<DataTypeMigration>("{82C4BA1D-7720-46B1-BBD7-07F3F73800E6}");
            Chain<TagsMigration>("{139F26D7-7E08-48E3-81D9-E50A21A72F67}");
            Chain<SuperZero>("{CC1B1201-1328-443C-954A-E0BBB8CCC1B5}");
            Chain<PropertyEditorsMigration>("{CA7DB949-3EF4-403D-8464-F9BA36A52E87}");
            Chain<LanguageColumns>("{7F0BF916-F64E-4B25-864A-170D6E6B68E5}");

            // merging from 7.8.0
            Chain<AddUserLoginTable>("{FDCB727A-EFB6-49F3-89E4-A346503AB849}");
            Chain<AddTourDataUserColumn>("{2A796A08-4FE4-4783-A1A5-B8A6C8AA4A92}");
            Chain<AddMediaVersionTable>("{1A46A98B-2AAB-4C8E-870F-A2D55A97FD1F}");
            Chain<AddInstructionCountColumn>("{0AE053F6-2683-4234-87B2-E963F8CE9498}");
            Chain<AddIndexToPropertyTypeAliasColumn>("{D454541C-15C5-41CF-8109-937F26A78E71}");

            // merging from 7.9.0
            Chain<AddIsSensitiveMemberTypeColumn>("{89A728D1-FF4C-4155-A269-62CC09AD2131}");
            Chain<AddUmbracoAuditTable>("{FD8631BC-0388-425C-A451-5F58574F6F05}");
            Chain<AddUmbracoConsentTable>("{2821F53E-C58B-4812-B184-9CD240F990D7}");
            Chain<CreateSensitiveDataUserGroup>("{8918450B-3DA0-4BB7-886A-6FA8B7E4186E}");

            // mergin from 7.10.0
            Chain<RenamePreviewFolder>("{79591E91-01EA-43F7-AC58-7BD286DB1E77}");

            // 8.0.0
            // AddVariationTables1 has been superceeded by AddVariationTables2
            //Chain<AddVariationTables1>("{941B2ABA-2D06-4E04-81F5-74224F1DB037}");
            Chain<AddVariationTables2>("{76DF5CD7-A884-41A5-8DC6-7860D95B1DF5}");

            // however, need to take care of ppl in post-AddVariationTables1 state
            Add<AddVariationTables1A>("{941B2ABA-2D06-4E04-81F5-74224F1DB037}", "{76DF5CD7-A884-41A5-8DC6-7860D95B1DF5}");

            // 8.0.0
            Chain<RefactorMacroColumns>("{A7540C58-171D-462A-91C5-7A9AA5CB8BFD}");
            
            // merge
            Chain<UserForeignKeys>("{3E44F712-E2E3-473A-AE49-5D7F8E67CE3F}"); // shannon added that one - let's keep it as the default path
            Chain<AddTypedLabels>("{4CACE351-C6B9-4F0C-A6BA-85A02BBD39E4}"); // then add stephan's - to new final state            
            //Chain<AddTypedLabels>("{65D6B71C-BDD5-4A2E-8D35-8896325E9151}"); // stephan added that one - need a path to final state
            Add<UserForeignKeys>("{65D6B71C-BDD5-4A2E-8D35-8896325E9151}", "{4CACE351-C6B9-4F0C-A6BA-85A02BBD39E4}");

            // 8.0.0
            Chain<ContentVariationMigration>("{1350617A-4930-4D61-852F-E3AA9E692173}");

            // FINAL STATE - MUST MATCH LAST ONE ABOVE !
            // whenever this changes, update all references in this file!

            Add(string.Empty, "{1350617A-4930-4D61-852F-E3AA9E692173}");
        }
    }
}
