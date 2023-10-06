using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

[TestFixture]
[Timeout(60000)]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class CustomDbContextUmbracoProviderTests : UmbracoIntegrationTest
{
    [Test]
    public void Can_Register_Custom_DbContext_And_Resolve()
    {
        var dbContext = Services.GetRequiredService<CustomDbContext>();

        Assert.IsNotNull(dbContext);
        Assert.IsNotEmpty(dbContext.Database.GetConnectionString());
    }

    protected override void ConfigureUmbracoTestServices(IUmbracoBuilder builder)
    {
        builder.Services.AddUmbracoDbContext<CustomDbContext>((serviceProvider, options) =>
        {
            options.UseUmbracoDatabaseProvider(serviceProvider);
        });
    }

    internal class CustomDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public CustomDbContext(DbContextOptions<CustomDbContext> options)
            : base(options)
        {
        }
    }
}


[TestFixture]
[Timeout(60000)]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class CustomDbContextCustomSqliteProviderTests : UmbracoIntegrationTest
{
    [Test]
    public void Can_Register_Custom_DbContext_And_Resolve()
    {
        var dbContext = Services.GetRequiredService<CustomDbContext>();

        Assert.IsNotNull(dbContext);
        Assert.IsNotEmpty(dbContext.Database.GetConnectionString());
    }

    protected override void ConfigureUmbracoTestServices(IUmbracoBuilder builder)
    {
        builder.Services.AddUmbracoDbContext<CustomDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite("Data Source=:memory:;Version=3;New=True;");
        });
    }

    internal class CustomDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public CustomDbContext(DbContextOptions<CustomDbContext> options)
            : base(options)
        {
        }
    }
}

[Obsolete]
[TestFixture]
[Timeout(60000)]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class CustomDbContextLegacyExtensionProviderTests : UmbracoIntegrationTest
{
    [Test]
    public void Can_Register_Custom_DbContext_And_Resolve()
    {
        var dbContext = Services.GetRequiredService<CustomDbContext>();

        Assert.IsNotNull(dbContext);
        Assert.IsNotEmpty(dbContext.Database.GetConnectionString());
    }

    protected override void ConfigureUmbracoTestServices(IUmbracoBuilder builder)
    {
        builder.Services.AddUmbracoEFCoreContext<CustomDbContext>("Data Source=:memory:;Version=3;New=True;", "Microsoft.Data.Sqlite", (options, connectionString, providerName) =>
        {
            options.UseSqlite(connectionString);
        });
    }

    internal class CustomDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public CustomDbContext(DbContextOptions<CustomDbContext> options)
            : base(options)
        {
        }
    }
}

