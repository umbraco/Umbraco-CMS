// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs
{
    public class DropForeignKeyMigrationStub : MigrationBase
    {
        public DropForeignKeyMigrationStub(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
        {
            Delete.ForeignKey().FromTable("umbracoUser2app").ForeignColumn("user").ToTable("umbracoUser").PrimaryColumn("id").Do();
        }
    }
}
