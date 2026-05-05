// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_4_0;

/// <summary>
/// Adds triggerSource, triggerOperation and logTypeAlias columns to the umbracoLog table
/// to support audit trigger context and custom audit type aliases.
/// </summary>
public class AddAuditLogTriggerAndTypeAlias : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddAuditLogTriggerAndTypeAlias"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddAuditLogTriggerAndTypeAlias(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override async Task MigrateAsync()
    {
        if (TableExists(LogDto.TableName) is false)
        {
            return;
        }

        var columns = Context.SqlContext.SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        AddColumnIfNotExists<LogDto>(columns, LogDto.TableName, "triggerSource");
        AddColumnIfNotExists<LogDto>(columns, LogDto.TableName, "triggerOperation");
        AddColumnIfNotExists<LogDto>(columns, LogDto.TableName, "logTypeAlias");
    }
}
