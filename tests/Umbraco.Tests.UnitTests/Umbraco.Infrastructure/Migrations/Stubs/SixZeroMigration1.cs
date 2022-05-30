// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

public class SixZeroMigration1 : MigrationBase
{
    public SixZeroMigration1(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() => Alter.Table("umbracoUser").AddColumn("secret").AsString(255);
}
