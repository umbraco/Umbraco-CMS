// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

public class FourElevenMigration : MigrationBase
{
    public FourElevenMigration(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() => Alter.Table("umbracoUser").AddColumn("companyPhone").AsString(255);
}
