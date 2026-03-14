// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

/// <summary>
/// Represents a stub migration class used in unit tests for simulating the dropping of foreign keys.
/// </summary>
public class DropForeignKeyMigrationStub : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DropForeignKeyMigrationStub"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public DropForeignKeyMigrationStub(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() => Delete.ForeignKey().FromTable("umbracoUser2app").ForeignColumn("user")
        .ToTable("umbracoUser").PrimaryColumn("id").Do();
}
