// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

    /// <summary>
    /// Represents a test stub migration used for simulating migrations to version 5.0 in unit tests.
    /// </summary>
public class FiveZeroMigration : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FiveZeroMigration"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for migration operations.</param>
    public FiveZeroMigration(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
    }
}
