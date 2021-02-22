// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Core.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs
{
    public class AlterUserTableMigrationStub : MigrationBase
    {
        public AlterUserTableMigrationStub(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
        {
            Alter.Table("umbracoUser")
                 .AddColumn("Birthday")
                 .AsDateTime()
                 .Nullable();
        }
    }
}
