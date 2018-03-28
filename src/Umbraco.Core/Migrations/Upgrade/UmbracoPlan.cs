using System;
using System.Configuration;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
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
        public UmbracoPlan()
            : base(Constants.System.UmbracoUpgradePlanName)
        {
            DefinePlan();
        }

        public UmbracoPlan(IMigrationBuilder migrationBuilder, ILogger logger)
            : base(Constants.System.UmbracoUpgradePlanName, migrationBuilder, logger)
        {
            DefinePlan();
        }

        public override string InitialState
        {
            get
            {
                // no state in database yet - assume we have something in web.config that makes some sense
                if (!SemVersion.TryParse(ConfigurationManager.AppSettings["umbracoConfigurationStatus"], out var currentVersion))
                    throw new InvalidOperationException("Could not get current version from web.config umbracoConfigurationStatus appSetting.");

                // must be at least 7.?.? - fixme adjust when releasing
                if (currentVersion < new SemVersion(7, 999))
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

        private void DefinePlan()
        {
            // UPGRADE FROM 7
            //
            // when 8.0.0 is released, on the first upgrade, the state is automatically
            // set to {init-7.x.y} where 7.x.y is the version (see above), and then we define upgrades.
            //
            // then, as more v7 and v8 versions are released, new chains needs to be defined to
            // support the upgrades (new v7 may backport some migrations and require their own
            // upgrade paths, etc).
            // fixme adjust when releasing

            From("{init-7.8.0}");
            Chain<V_8_0_0.AddLockObjects>("{7C447271-CA3F-4A6A-A913-5D77015655CB}"); // add more lock objects
            Chain<AddContentNuTable>("{CBFF58A2-7B50-4F75-8E98-249920DB0F37}");
            Chain<RefactorXmlColumns>("{3D18920C-E84D-405C-A06A-B7CEE52FE5DD}");
            Chain<VariantsMigration>("{FB0A5429-587E-4BD0-8A67-20F0E7E62FF7}");
            Chain<DropMigrationsTable>("{F0C42457-6A3B-4912-A7EA-F27ED85A2092}");
            Chain<DataTypeMigration>("{8640C9E4-A1C0-4C59-99BB-609B4E604981}");
            Chain<TagsMigration>("{DD1B99AF-8106-4E00-BAC7-A43003EA07F8}");
            Chain<SuperZero>("{9DF05B77-11D1-475C-A00A-B656AF7E0908}");
            Chain<PropertyEditorsMigration>("{CA7DB949-3EF4-403D-8464-F9BA36A52E87}");

            // 7.8.1 = same as 7.8.0
            From("{init-7.8.1}");
            Chain("{init-7.8.0}");

            // 7.9.0 = requires its own chain
            From("{init-7.9.0}");
            // chain...
            Chain("{82C4BA1D-7720-46B1-BBD7-07F3F73800E6}");


            // UPGRADE 8
            //
            // starting from the original 8.0.0 final state, chain migrations to upgrade version 8,
            // defining new final states as more migrations are added to the chain.
            //
            // before v8 is released, some sites may exist, and these "pre-8" versions require their
            // own upgrade plan. in other words, this is also the plan for sites that were on v8 before
            // v8 was released

            // 8.0.0
            From("{init-origin}");
            Chain<V_8_0_0.AddLockTable>("{98347B5E-65BF-4DD7-BB43-A09CB7AF4FCA}");
            Chain<V_8_0_0.AddLockObjects>("{1E8165C4-942D-40DC-AC76-C5FF8831E400}");
            Chain<AddContentNuTable>("{39E15568-7AAD-4D54-81D0-758CCFC529F8}");
            Chain<RefactorXmlColumns>("{55C3F97D-BDA7-4FB1-A743-B0456B56EAA3}");

            // 7.5.0
            Chain<RemoveStylesheetDataAndTablesAgain>("{287F9E39-F673-42F7-908C-21659AB13B13}");
            Chain<V_7_5_0.UpdateUniqueIndexOnPropertyData>("{2D08588A-AD90-479C-9F6E-A99B60BA7226}");
            Chain<AddRedirectUrlTable>("{2D917FF8-AC81-4C00-A407-1F4B1DF6089C}");

            // 7.5.5
            Chain<UpdateAllowedMediaTypesAtRoot>("{44484C32-EEB3-4A12-B1CB-11E02CE22AB2}");

            // 7.6.0
            Chain<AddIndexesToUmbracoRelationTables>("{3586E4E9-2922-49EB-8E2A-A530CE6DBDE0}");
            Chain<AddIndexToCmsMemberLoginName>("{D4A5674F-654D-4CC7-85E5-CFDBC533A318}");
            Chain<AddIndexToUmbracoNodePath>("{7F828EDD-6622-4A8D-AD80-EEAF46C11680}");
            Chain<AddMacroUniqueIdColumn>("{F30AC223-D277-4D1F-B2AB-F0F0D3546CE1}");
            Chain<AddRelationTypeUniqueIdColumn>("{7C27E310-CF48-4637-A22E-8D87355161C1}");
            Chain<NormalizeTemplateGuids>("{7D2ABA16-EE48-4569-8827-E81370FC4871}");
            Chain<ReduceLoginNameColumnsSize>("{02879EDF-13A8-43AF-87A5-DD85723D0016}");
            Chain<V_7_6_0.UpdateUniqueIndexOnPropertyData>("{5496C6CC-3AE0-4789-AF49-5BB4E28FA424}");
            Chain<RemoveUmbracoDeployTables>("{8995332B-085E-4C0C-849E-9A77E79F4293}");

            // 7.7.0
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

            // at this point of the chain, people started to work on v8, so whenever we
            // merge stuff from v7, we have to chain the migrations here so they also
            // run for v8.

            // mergin from 7.8.0
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
            Chain<LanguageColumns>("FIXGUID NEW FINAL");

            // FINAL STATE - MUST MATCH LAST ONE ABOVE !

            Add(string.Empty, "{8918450B-3DA0-4BB7-886A-6FA8B7E4186E}");
        }
    }
}
