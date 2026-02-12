// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Migrations;
using Constants = Umbraco.Cms.Infrastructure.Persistence.EFCore.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

internal class TestSqliteMigrationProviderSetup : IMigrationProviderSetup
{
    public string ProviderName => Constants.ProviderNames.SQLLite;

    public void Setup(DbContextOptionsBuilder builder, string? connectionString)
        => builder.UseSqlite(connectionString);
}

internal class TestSqlServerMigrationProviderSetup : IMigrationProviderSetup
{
    public string ProviderName => Constants.ProviderNames.SQLServer;

    public void Setup(DbContextOptionsBuilder builder, string? connectionString)
        => builder.UseSqlServer(connectionString);
}
