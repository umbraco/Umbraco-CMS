using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_20_0_0;

/// <summary>
/// Adds the <c>umbracoContentDraftBranch</c> table and the <c>branchId</c> column on <c>umbracoContentVersion</c>,
/// laying the data-model groundwork for named draft branches (multiple simultaneous named drafts per content item).
/// </summary>
public class AddContentDraftBranches : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddContentDraftBranches"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddContentDraftBranches(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        EnsureContentDraftBranchTable();
        EnsureContentVersionBranchIdColumn();
        EnsureContentDraftBranchesLock();
        EnsureScheduledPublishDateColumn();
        EnsureScheduledCulturesColumn();
        return Task.CompletedTask;
    }

    private void EnsureContentDraftBranchTable()
    {
        if (!TableExists(Constants.DatabaseSchema.Tables.ContentDraftBranch))
        {
            Create.Table<ContentDraftBranchDto>().Do();
        }
    }

    private void EnsureScheduledPublishDateColumn()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        if (columns.Any(x => x.TableName.InvariantEquals(Constants.DatabaseSchema.Tables.ContentDraftBranch)
                             && x.ColumnName.InvariantEquals("scheduledPublishDate")) == false)
        {
            AddColumn<ContentDraftBranchDto>(Constants.DatabaseSchema.Tables.ContentDraftBranch, "scheduledPublishDate");
        }
    }

    private void EnsureScheduledCulturesColumn()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        if (columns.Any(x => x.TableName.InvariantEquals(Constants.DatabaseSchema.Tables.ContentDraftBranch)
                             && x.ColumnName.InvariantEquals("scheduledCultures")) == false)
        {
            AddColumn<ContentDraftBranchDto>(Constants.DatabaseSchema.Tables.ContentDraftBranch, "scheduledCultures");
        }
    }

    private void EnsureContentDraftBranchesLock()
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.ContentDraftBranches);

        LockDto? contentDraftBranchesLock = Database.Fetch<LockDto>(sql).FirstOrDefault();

        if (contentDraftBranchesLock is null)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.ContentDraftBranches, Name = "ContentDraftBranches" });
        }
    }

    private void EnsureContentVersionBranchIdColumn()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        if (columns.Any(x => x.TableName.InvariantEquals(Constants.DatabaseSchema.Tables.ContentVersion)
                             && x.ColumnName.InvariantEquals("branchId")) == false)
        {
            AddColumn<ContentVersionDto>(Constants.DatabaseSchema.Tables.ContentVersion, "branchId");
        }
    }
}
