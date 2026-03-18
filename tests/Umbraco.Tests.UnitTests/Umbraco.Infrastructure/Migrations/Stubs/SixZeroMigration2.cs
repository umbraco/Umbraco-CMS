// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

/// <summary>
/// A stub implementation of a migration class used for unit testing in the Umbraco CMS infrastructure.
/// This class is named SixZeroMigration2 to represent a specific migration scenario in tests.
/// </summary>
public class SixZeroMigration2 : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs.SixZeroMigration2"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public SixZeroMigration2(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() => Alter.Table("umbracoUser").AddColumn("secondEmail").AsString(255);
}
