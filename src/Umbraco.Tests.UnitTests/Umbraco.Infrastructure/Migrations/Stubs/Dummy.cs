﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs
{
    /// <summary>
    /// This is just a dummy class that is used to ensure that implementations
    /// of IMigration is not found if it doesn't have the MigrationAttribute (like this class).
    /// </summary>
    public class Dummy : IMigration
    {
        public void Migrate()
        {
            throw new System.NotImplementedException();
        }
    }
}
