using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Upgrade.TargetVersionEight;
using Umbraco.Core.Migrations.Upgrade.TargetVersionSevenFiveFive;
using Umbraco.Core.Migrations.Upgrade.TargetVersionSevenFiveZero;
using Umbraco.Core.Migrations.Upgrade.TargetVersionSevenSevenZero;
using Umbraco.Core.Migrations.Upgrade.TargetVersionSevenSixZero;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Migrations.Upgrade
{
    /// <summary>
    /// Represents Umbraco's migration plan.
    /// </summary>
    public class UmbracoPlan : MigrationPlan
    {
        public UmbracoPlan(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, ILogger logger)
            : base("umbraco", scopeProvider, migrationBuilder, logger)
        {
            // INSTALL
            // when installing, the source state is empty, and the target state should be the installed state,
            // ie the common end state to all upgrade branches, representing the current version
            Add(string.Empty, "{6550C7E8-77B7-4DE3-9B58-E31C81CB9504}");

            // UPGRADE FROM 7
            // when 8.0.0 is released, on the first upgrade, the state is automatically
            // set to {init-7.x.y} where 7.x.y is the version, detected from the database.

            From("{init-7.8.0}")
                .Chain<TargetVersionEight.AddLockObjects>("{7C447271-CA3F-4A6A-A913-5D77015655CB}") // add more lock objects
                .Chain<AddContentNuTable>("{CBFF58A2-7B50-4F75-8E98-249920DB0F37}")
                .Chain<RefactorXmlColumns>("{3D18920C-E84D-405C-A06A-B7CEE52FE5DD}")
                .Chain<VariantsMigration>("{6550C7E8-77B7-4DE3-9B58-E31C81CB9504}");

            // UPGRADE FROM 7
            // after 8.0.0 has been released we are going to release more v7 versions, which
            // may backport some migrations, etc - so they would need their own upgrade path.

            // 7.8.1 = same as 7.8.0
            From("{init-7.8.1}")
                .Chain("{init-7.8.0}");

            // 7.9.0 = requires its own chain
            From("{init-7.9.0}")
                // chain...
                .Chain("{6550C7E8-77B7-4DE3-9B58-E31C81CB9504}");

            // UPGRADE 8
            // chain migrations for 8.0.1 etc
            // should start from the install state when 8.0.0 is released, and chain
            // migrations, and end with the new install state.

            //From("")
            //    .Chain("")
            //    .Chain("");

            // WIP 8
            // during v8 development, we have existing v8 sites that we want to upgrade,
            // and this requires merging in some v7 migrations. This chain should quite
            // probably never be released. NOTE that when adding a migration at the bottom
            // of this chain, one should update the installed state in the INSTALL and
            // UPGRADE chains above. And, once 8.0.0 has been released, this chain should
            // never ever change again.

            // 8.0.0
            From("{init-origin}");
            Chain<TargetVersionEight.AddLockTable>("{98347B5E-65BF-4DD7-BB43-A09CB7AF4FCA}");
            Chain<TargetVersionEight.AddLockObjects>("{1E8165C4-942D-40DC-AC76-C5FF8831E400}");
            Chain<AddContentNuTable>("{39E15568-7AAD-4D54-81D0-758CCFC529F8}");
            Chain<RefactorXmlColumns>("{55C3F97D-BDA7-4FB1-A743-B0456B56EAA3}");

            // 7.5.0
            Chain<RemoveStylesheetDataAndTablesAgain>("{287F9E39-F673-42F7-908C-21659AB13B13}");
            Chain<TargetVersionSevenFiveZero.UpdateUniqueIndexOnPropertyData>("{2D08588A-AD90-479C-9F6E-A99B60BA7226}");
            Chain<AddRedirectUrlTable>("{2D917FF8-AC81-4C00-A407-1F4B1DF6089C}");

            // 7.5.5
            Chain<UpdateAllowedMediaTypesAtRoot>("{44484C32-EEB3-4A12-B1CB-11E02CE22AB2}");

            // 7.6.0
            //Chain<AddLockTable>("{858B4039-070C-4928-BBEC-DDE8303352DA}");
            //Chain<AddLockObjects>("{64F587C1-0B28-4D78-B4CC-26B7D87F69C1}");
            Chain<AddIndexesToUmbracoRelationTables>("{3586E4E9-2922-49EB-8E2A-A530CE6DBDE0}");
            Chain<AddIndexToCmsMemberLoginName>("{D4A5674F-654D-4CC7-85E5-CFDBC533A318}");
            Chain<AddIndexToUmbracoNodePath>("{7F828EDD-6622-4A8D-AD80-EEAF46C11680}");
            Chain<AddMacroUniqueIdColumn>("{F30AC223-D277-4D1F-B2AB-F0F0D3546CE1}");
            Chain<AddRelationTypeUniqueIdColumn>("{7C27E310-CF48-4637-A22E-8D87355161C1}");
            Chain<NormalizeTemplateGuids>("{7D2ABA16-EE48-4569-8827-E81370FC4871}");
            Chain<ReduceLoginNameColumnsSize>("{02879EDF-13A8-43AF-87A5-DD85723D0016}");
            Chain<TargetVersionSevenSixZero.UpdateUniqueIndexOnPropertyData>("{5496C6CC-3AE0-4789-AF49-5BB4E28FA424}");
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
        }
    }
}
