// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

/// <summary>
///     This is just a dummy class that is used to ensure that implementations
///     of IMigration is not found if it doesn't have the MigrationAttribute (like this class).
/// </summary>
public class Dummy : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Dummy"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public Dummy(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() => throw new NotImplementedException();
}
