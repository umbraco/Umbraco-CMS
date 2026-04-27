// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

public class SixZeroMigration2 : AsyncMigrationBase
{
    public SixZeroMigration2(IMigrationContext context)
        : base(context)
    {
    }

    protected override async Task MigrateAsync() => Alter.Table("umbracoUser").AddColumn("secondEmail").AsString(255);
}
