// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_4_0;

/// <summary>
/// Migration to add the external member and external member to member group tables.
/// </summary>
public class AddExternalMemberTables : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddExternalMemberTables"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddExternalMemberTables(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override async Task MigrateAsync()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.ExternalMember) is false)
        {
            Create.Table<ExternalMemberDto>().Do();
        }

        if (TableExists(Constants.DatabaseSchema.Tables.ExternalMember2MemberGroup) is false)
        {
            Create.Table<ExternalMember2MemberGroupDto>().Do();
        }
    }
}
