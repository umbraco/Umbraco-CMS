// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Core.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs
{
    public class SixZeroMigration2 : MigrationBase
    {
        public SixZeroMigration2(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
        {
            Alter.Table("umbracoUser").AddColumn("secondEmail").AsString(255);
        }
    }
}
