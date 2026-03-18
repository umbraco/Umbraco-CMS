// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

/// <summary>
/// Represents a stub migration used for testing scenarios that involve altering the user table.
/// </summary>
public class AlterUserTableMigrationStub : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlterUserTableMigrationStub"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AlterUserTableMigrationStub(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() =>
        Alter.Table("umbracoUser")
            .AddColumn("Birthday")
            .AsDateTime()
            .Nullable();
}
