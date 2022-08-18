using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.Common;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_0_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_2_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_1;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_1_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_10_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_15_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_17_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_6_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_7_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_9_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_1_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_2_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_3_0;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_4_0;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade;

/// <summary>
///     Represents the Umbraco CMS migration plan.
/// </summary>
/// <seealso cref="MigrationPlan" />
public class UmbracoPlan : MigrationPlan
{
    private const string InitPrefix = "{init-";
    private const string InitSuffix = "}";
    private readonly IUmbracoVersion _umbracoVersion;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoPlan" /> class.
    /// </summary>
    /// <param name="umbracoVersion">The Umbraco version.</param>
    public UmbracoPlan(IUmbracoVersion umbracoVersion)
        : base(Constants.Conventions.Migrations.UmbracoUpgradePlanName)
    {
        _umbracoVersion = umbracoVersion;
        DefinePlan();
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <para>The default initial state in plans is string.Empty.</para>
    ///     <para>
    ///         When upgrading from version 7, we want to use specific initial states
    ///         that are e.g. "{init-7.9.3}", "{init-7.11.1}", etc. so we can chain the proper
    ///         migrations.
    ///     </para>
    ///     <para>
    ///         This is also where we detect the current version, and reject invalid
    ///         upgrades (from a tool old version, or going back in time, etc).
    ///     </para>
    /// </remarks>
    public override string InitialState
    {
        get
        {
            SemVersion currentVersion = _umbracoVersion.SemanticVersion;

            // only from 8.0.0 and above
            var minVersion = new SemVersion(8);
            if (currentVersion < minVersion)
            {
                throw new InvalidOperationException(
                    $"Version {currentVersion} cannot be migrated to {_umbracoVersion.SemanticVersion}."
                    + $" Please upgrade first to at least {minVersion}.");
            }

            // Force versions between 7.14.*-7.15.* into into 7.14 initial state. Because there is no db-changes,
            // and we don't want users to workaround my putting in version 7.14.0 them self.
            if (minVersion <= currentVersion && currentVersion < new SemVersion(7, 16))
            {
                return GetInitState(minVersion);
            }

            // initial state is eg "{init-7.14.0}"
            return GetInitState(currentVersion);
        }
    }

    /// <inheritdoc />
    public override void ThrowOnUnknownInitialState(string state)
    {
        if (TryGetInitStateVersion(state, out var initVersion))
        {
            throw new InvalidOperationException(
                $"Version {_umbracoVersion.SemanticVersion} does not support migrating from {initVersion}."
                + $" Please verify which versions support migrating from {initVersion}.");
        }

        base.ThrowOnUnknownInitialState(state);
    }

    /// <summary>
    ///     Gets the initial state corresponding to a version.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <returns>
    ///     The initial state.
    /// </returns>
    private static string GetInitState(SemVersion version) => InitPrefix + version + InitSuffix;

    /// <summary>
    ///     Tries to extract a version from an initial state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="version">The version.</param>
    /// <returns>
    ///     <c>true</c> when the state contains a version; otherwise, <c>false</c>.D
    /// </returns>
    private static bool TryGetInitStateVersion(string state, [MaybeNullWhen(false)] out string version)
    {
        if (state.StartsWith(InitPrefix) && state.EndsWith(InitSuffix))
        {
            version = state.TrimStart(InitPrefix).TrimEnd(InitSuffix);
            return true;
        }

        version = null;
        return false;
    }

    /// <summary>
    ///     Defines the plan.
    /// </summary>
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
        //     out of the conflict states (see examples below).
        //
        // * Porting from version 7:
        //     Append the ported migration to the main chain, using a new guid (same as above).
        //     Create a new special chain from the {init-...} state to the main chain.

        // plan starts at 7.14.0 (anything before 7.14.0 is not supported)
        From(GetInitState(new SemVersion(7, 14)));

        // begin migrating from v7 - remove all keys and indexes
        To<DeleteKeysAndIndexes>("{B36B9ABD-374E-465B-9C5F-26AB0D39326F}");

        To<AddLockObjects>("{7C447271-CA3F-4A6A-A913-5D77015655CB}");
        To<AddContentNuTable>("{CBFF58A2-7B50-4F75-8E98-249920DB0F37}");
        To<RenameMediaVersionTable>("{5CB66059-45F4-48BA-BCBD-C5035D79206B}");
        To<VariantsMigration>("{FB0A5429-587E-4BD0-8A67-20F0E7E62FF7}");
        To<DropMigrationsTable>("{F0C42457-6A3B-4912-A7EA-F27ED85A2092}");
        To<DataTypeMigration>("{8640C9E4-A1C0-4C59-99BB-609B4E604981}");
        To<TagsMigration>("{DD1B99AF-8106-4E00-BAC7-A43003EA07F8}");
        To<SuperZero>("{9DF05B77-11D1-475C-A00A-B656AF7E0908}");
        To<PropertyEditorsMigration>("{6FE3EF34-44A0-4992-B379-B40BC4EF1C4D}");
        To<LanguageColumns>("{7F59355A-0EC9-4438-8157-EB517E6D2727}");
        ToWithReplace<AddVariationTables2, AddVariationTables1A>(
            "{941B2ABA-2D06-4E04-81F5-74224F1DB037}",
            "{76DF5CD7-A884-41A5-8DC6-7860D95B1DF5}"); // kill AddVariationTable1
        To<RefactorMacroColumns>("{A7540C58-171D-462A-91C5-7A9AA5CB8BFD}");

        Merge()
            .To<UserForeignKeys>("{3E44F712-E2E3-473A-AE49-5D7F8E67CE3F}")
            .With()
            .To<AddTypedLabels>("{65D6B71C-BDD5-4A2E-8D35-8896325E9151}")
            .As("{4CACE351-C6B9-4F0C-A6BA-85A02BBD39E4}");

        To<ContentVariationMigration>("{1350617A-4930-4D61-852F-E3AA9E692173}");
        To<FallbackLanguage>("{CF51B39B-9B9A-4740-BB7C-EAF606A7BFBF}");
        To<UpdateDefaultMandatoryLanguage>("{5F4597F4-A4E0-4AFE-90B5-6D2F896830EB}");
        To<RefactorVariantsModel>("{290C18EE-B3DE-4769-84F1-1F467F3F76DA}");
        To<DropTaskTables>("{6A2C7C1B-A9DB-4EA9-B6AB-78E7D5B722A7}");
        To<AddLogTableColumns>("{8804D8E8-FE62-4E3A-B8A2-C047C2118C38}");
        To<DropPreValueTable>("{23275462-446E-44C7-8C2C-3B8C1127B07D}");
        To<DropDownPropertyEditorsMigration>("{6B251841-3069-4AD5-8AE9-861F9523E8DA}");
        To<TagsMigrationFix>("{EE429F1B-9B26-43CA-89F8-A86017C809A3}");
        To<DropTemplateDesignColumn>("{08919C4B-B431-449C-90EC-2B8445B5C6B1}");
        To<TablesForScheduledPublishing>("{7EB0254C-CB8B-4C75-B15B-D48C55B449EB}");
        To<MakeTagsVariant>("{C39BF2A7-1454-4047-BBFE-89E40F66ED63}");
        To<MakeRedirectUrlVariant>("{64EBCE53-E1F0-463A-B40B-E98EFCCA8AE2}");
        To<AddContentTypeIsElementColumn>("{0009109C-A0B8-4F3F-8FEB-C137BBDDA268}");
        To<ConvertRelatedLinksToMultiUrlPicker>("{ED28B66A-E248-4D94-8CDB-9BDF574023F0}");
        To<UpdatePickerIntegerValuesToUdi>("{38C809D5-6C34-426B-9BEA-EFD39162595C}");
        To<RenameUmbracoDomainsTable>("{6017F044-8E70-4E10-B2A3-336949692ADD}");

        Merge()
            .To<DropXmlTables>("{CDBEDEE4-9496-4903-9CF2-4104E00FF960}")
            .With()
            .To<RadioAndCheckboxPropertyEditorsMigration>("{940FD19A-00A8-4D5C-B8FF-939143585726}")
            .As("{0576E786-5C30-4000-B969-302B61E90CA3}");

        To<FixLanguageIsoCodeLength>("{48AD6CCD-C7A4-4305-A8AB-38728AD23FC5}");
        To<AddPackagesSectionAccess>("{DF470D86-E5CA-42AC-9780-9D28070E25F9}");

        // finish migrating from v7 - recreate all keys and indexes
        To<CreateKeysAndIndexes>("{3F9764F5-73D0-4D45-8804-1240A66E43A2}");

        To<RenameLabelAndRichTextPropertyEditorAliases>("{E0CBE54D-A84F-4A8F-9B13-900945FD7ED9}");
        To<MergeDateAndDateTimePropertyEditor>("{78BAF571-90D0-4D28-8175-EF96316DA789}");

        // release-8.0.0

        // to 8.0.1
        To<ChangeNuCacheJsonFormat>("{80C0A0CB-0DD5-4573-B000-C4B7C313C70D}");

        // release-8.0.1

        // to 8.1.0
        To<ConvertTinyMceAndGridMediaUrlsToLocalLink>("{B69B6E8C-A769-4044-A27E-4A4E18D1645A}");
        To<RenameUserLoginDtoDateIndex>("{0372A42B-DECF-498D-B4D1-6379E907EB94}");
        To<FixContentNuCascade>("{5B1E0D93-F5A3-449B-84BA-65366B84E2D4}");

        // to 8.6.0
        To<UpdateRelationTypeTable>("{4759A294-9860-46BC-99F9-B4C975CAE580}");
        To<AddNewRelationTypes>("{0BC866BC-0665-487A-9913-0290BD0169AD}");
        To<AddPropertyTypeValidationMessageColumns>("{3D67D2C8-5E65-47D0-A9E1-DC2EE0779D6B}");
        To<MissingContentVersionsIndexes>("{EE288A91-531B-4995-8179-1D62D9AA3E2E}");
        To<AddMainDomLock>("{2AB29964-02A1-474D-BD6B-72148D2A53A2}");

        // to 8.7.0
        To<MissingDictionaryIndex>("{a78e3369-8ea3-40ec-ad3f-5f76929d2b20}");

        // to 8.9.0
        To<ExternalLoginTableUserData>("{B5838FF5-1D22-4F6C-BCEB-F83ACB14B575}");

        // to 8.10.0
        To<AddPropertyTypeLabelOnTopColumn>("{D6A8D863-38EC-44FB-91EC-ACD6A668BD18}");

        // NOTE: we need to do a merge migration here because as of 'now',
        // v9-beta* is already out and 8.15 isn't out yet
        // so we need to ensure that migrations from 8.15 are included in the next
        // v9*.

        // to 8.15.0
        To<AddCmsContentNuByteColumn>("{8DDDCD0B-D7D5-4C97-BD6A-6B38CA65752F}");
        To<UpgradedIncludeIndexes>("{4695D0C9-0729-4976-985B-048D503665D8}");
        To<UpdateCmsPropertyGroupIdSeed>("{5C424554-A32D-4852-8ED1-A13508187901}");

        // to 8.17.0
        To<AddPropertyTypeGroupColumns>("{153865E9-7332-4C2A-9F9D-F20AEE078EC7}");

        // Hack to support migration from 8.18
        To<NoopMigration>("{03482BB0-CF13-475C-845E-ECB8319DBE3C}");

        // This should be safe to execute again. We need it with a new name to ensure updates from all the following has executed this step.
        // - 8.15.0 RC    - Current state: {4695D0C9-0729-4976-985B-048D503665D8}
        // - 8.15.0 Final - Current state: {5C424554-A32D-4852-8ED1-A13508187901}
        // - 9.0.0 RC1    - Current state: {5060F3D2-88BE-4D30-8755-CF51F28EAD12}
        To<UpdateCmsPropertyGroupIdSeed>("{622E5172-42E1-4662-AD80-9504AF5A4E53}");
        To<ExternalLoginTableIndexesFixup>("{10F7BB61-C550-426B-830B-7F954F689CDF}");
        To<DictionaryTablesIndexes>("{5AAE6276-80DB-4ACF-B845-199BC6C37538}");

        // to 9.0.0 RC1
        To<MigrateLogViewerQueriesFromFileToDb>("{22D801BA-A1FF-4539-BFCC-2139B55594F8}");
        To<ExternalLoginTableIndexes>("{50A43237-A6F4-49E2-A7A6-5DAD65C84669}");
        To<ExternalLoginTokenTable>("{3D8DADEF-0FDA-4377-A5F0-B52C2110E8F2}");
        To<MemberTableColumns>("{1303BDCF-2295-4645-9526-2F32E8B35ABD}");
        To<AddPasswordConfigToMemberTable>("{5060F3D2-88BE-4D30-8755-CF51F28EAD12}");
        To<AddPropertyTypeGroupColumns>(
            "{A2686B49-A082-4B22-97FD-AAB154D46A57}"); // Re-run this migration to make sure it has executed to account for migrations going out of sync between versions.

        // TO 9.0.0-rc4
        To<UmbracoServerColumn>(
            "5E02F241-5253-403D-B5D3-7DB00157E20F"); // Jaddie: This GUID is missing the { }, although this likely can't be changed now as it will break installs going forwards

        // TO 9.1.0
        To<AddContentVersionCleanupFeature>("{8BAF5E6C-DCB7-41AE-824F-4215AE4F1F98}");

        // TO 9.2.0
        To<AddUserGroup2NodeTable>("{0571C395-8F0B-44E9-8E3F-47BDD08D817B}");
        To<AddDefaultForNotificationsToggle>("{AD3D3B7F-8E74-45A4-85DB-7FFAD57F9243}");

        // TO 9.3.0
        To<MovePackageXMLToDb>("{A2F22F17-5870-4179-8A8D-2362AA4A0A5F}");
        To<UpdateExternalLoginToUseKeyInsteadOfId>("{CA7A1D9D-C9D4-4914-BC0A-459E7B9C3C8C}");
        To<AddTwoFactorLoginTable>("{0828F206-DCF7-4F73-ABBB-6792275532EB}");

        // TO 9.4.0
        To<AddScheduledPublishingLock>("{DBBA1EA0-25A1-4863-90FB-5D306FB6F1E1}");
        To<UpdateRelationTypesToHandleDependencies>("{DED98755-4059-41BB-ADBD-3FEAB12D1D7B}");

        // TO 10.0.0
        To<AddMemberPropertiesAsColumns>("{B7E0D53C-2B0E-418B-AB07-2DDE486E225F}");

        // TO 10.2.0
        To<AddUserGroup2LanguageTable>("{D0B3D29D-F4D5-43E3-BA67-9D49256F3266}");
        To<AddHasAccessToAllLanguagesColumn>("{79D8217B-5920-4C0E-8E9A-3CF8FA021882}");
    }
}
