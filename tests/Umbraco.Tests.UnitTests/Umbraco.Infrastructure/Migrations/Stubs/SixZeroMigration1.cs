// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

/// <summary>
/// Stub migration class used for unit testing in the Umbraco CMS test suite.
/// </summary>
public class SixZeroMigration1 : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SixZeroMigration1"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public SixZeroMigration1(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() => Alter.Table("umbracoUser").AddColumn("secret").AsString(255);
}
