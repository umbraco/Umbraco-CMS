using NPoco;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_4_0;

/// <summary>
///     Creates missing tabs on content types when a tab is referenced by both a composition and the content type's own groups.
/// </summary>
/// <remarks>
///     In v13, if a tab had groups in both a composition and the content type, the tab might not exist on the content type itself.
///     Newer versions require such tabs to also exist directly on the content type. This migration ensures those tabs are created.
/// </remarks>
[Obsolete("Remove in Umbraco 18.")]
public class CreateMissingTabs : UnscopedAsyncMigrationBase
{
    private readonly IScopeProvider _scopeProvider;

    public CreateMissingTabs(IMigrationContext context, IScopeProvider scopeProvider)
        : base(context) => _scopeProvider = scopeProvider;

    protected override async Task MigrateAsync()
    {
        using IScope scope = _scopeProvider.CreateScope();

        // 1. Find all property groups (type 0) and extract their tab alias (the part before the first '/').
        //    This helps identify which groups are referencing tabs.
        Sql<ISqlContext> groupsSql = Database.SqlContext.Sql()
            .SelectDistinct<PropertyTypeGroupDto>("g", pt => pt.ContentTypeNodeId)
            .AndSelect(GetTabAliasQuery("g.alias") + " AS tabAlias")
            .From<PropertyTypeDto>(alias: "p")
            .InnerJoin<PropertyTypeGroupDto>(alias: "g").On<PropertyTypeDto, PropertyTypeGroupDto>(
                (pt, ptg) => pt.PropertyTypeGroupId == ptg.Id && pt.ContentTypeId == ptg.ContentTypeNodeId,
                aliasLeft: "p",
                "g")
            .Where<PropertyTypeGroupDto>(x => x.Type == 0, alias: "g")
            .Where(CheckIfContainsTabAliasQuery("g.alias"));

        // 2. Get all existing tabs (type 1) for all content types.
        Sql<ISqlContext> tabsSql = Database.SqlContext.Sql()
            .Select<PropertyTypeGroupDto>("g2", g => g.UniqueId, g => g.ContentTypeNodeId, g => g.Alias)
            .From<PropertyTypeGroupDto>(alias: "g2")
            .Where<PropertyTypeGroupDto>(x => x.Type == 1, alias: "g2");

        // 3. Identify groups that reference a tab alias which does not exist as a tab for their content type.
        //    These are the "missing tabs" that need to be created.
        Sql<ISqlContext> missingTabsSql = Database.SqlContext.Sql()
            .Select<PropertyTypeGroupDto>("groups", g => g.ContentTypeNodeId)
            .AndSelect("groups.tabAlias")
            .From()
            .AppendSubQuery(groupsSql, "groups")
            .LeftJoin(tabsSql, "tabs")
            .On("groups.ContentTypeNodeId = tabs.ContentTypeNodeId AND tabs.alias = groups.tabAlias")
            .WhereNull<PropertyTypeGroupDto>(ptg => ptg.UniqueId, "tabs");

        // 4. For each missing tab, find the corresponding tab details (text, alias, sort order)
        //    from the parent content type (composition) that already has this tab.
        Sql<ISqlContext> missingTabsWithDetailsSql = Database.SqlContext.Sql()
            .Select<PropertyTypeGroupDto>("missingTabs", ptg => ptg.ContentTypeNodeId)
            .AndSelect<PropertyTypeGroupDto>("tg", ptg => ptg.Alias)
            .AndSelect("MIN(text) AS text", "MIN(sortorder) AS sortOrder")
            .From()
            .AppendSubQuery(missingTabsSql, "missingTabs")
            .InnerJoin<ContentType2ContentTypeDto>(alias: "ct2ct")
            .On<PropertyTypeGroupDto, ContentType2ContentTypeDto>(
                (ptg, ct2Ct) => ptg.ContentTypeNodeId == ct2Ct.ChildId,
                "missingTabs",
                "ct2ct")
            .InnerJoin<PropertyTypeGroupDto>(alias: "tg")
            .On<ContentType2ContentTypeDto, PropertyTypeGroupDto>(
                (ct2Ct, ptg) => ct2Ct.ParentId == ptg.ContentTypeNodeId,
                "ct2ct",
                "tg")
            .Append("AND tg.alias = missingTabs.tabAlias")
            .GroupBy<PropertyTypeGroupDto>("missingTabs", ptg => ptg.ContentTypeNodeId)
            .AndBy<PropertyTypeGroupDto>("tg", ptg => ptg.Alias);

        List<MissingTabWithDetails> missingTabsWithDetails =
            await Database.FetchAsync<MissingTabWithDetails>(missingTabsWithDetailsSql);

        // 5. Create and insert new tab records for each missing tab, using the details from the parent/composition.
        IEnumerable<PropertyTypeGroupDto> newTabs = missingTabsWithDetails
            .Select(missingTabWithDetails => new PropertyTypeGroupDto
            {
                UniqueId = Guid.CreateVersion7(),
                ContentTypeNodeId = missingTabWithDetails.ContentTypeNodeId,
                Type = 1,
                Text = missingTabWithDetails.Text,
                Alias = missingTabWithDetails.Alias,
                SortOrder = missingTabWithDetails.SortOrder,
            });
        await Database.InsertBatchAsync(newTabs);

        // 6. Complete the migration and commit the transaction.
        Context.Complete();
        scope.Complete();
    }

    private string GetTabAliasQuery(string columnName) =>
        DatabaseType == DatabaseType.SQLite
            ? $"substr({columnName}, 1, INSTR({columnName},'/') - 1)"
            : $"SUBSTRING({columnName}, 1, CHARINDEX('/', {columnName}) - 1)";

    private string CheckIfContainsTabAliasQuery(string columnName) =>
        DatabaseType == DatabaseType.SQLite
            ? $"INSTR({columnName}, '/') > 0"
            : $"CHARINDEX('/', {columnName}) > 0";

    private class MissingTabWithDetails
    {
        public required int ContentTypeNodeId { get; set; }

        public required string Alias { get; set; }

        public required string Text { get; set; }

        public required int SortOrder { get; set; }
    }
}
