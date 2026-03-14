// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

/// <summary>
/// Represents a database migration specific to Umbraco version 4.11, used for testing migration scenarios.
/// </summary>
public class FourElevenMigration : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FourElevenMigration"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public FourElevenMigration(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() => Alter.Table("umbracoUser").AddColumn("companyPhone").AsString(255);
}
