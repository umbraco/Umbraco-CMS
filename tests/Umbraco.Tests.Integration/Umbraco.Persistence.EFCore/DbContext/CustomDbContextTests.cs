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
public class CustomDbContextTests : UmbracoIntegrationTest
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
        builder.Services.AddUmbracoEFCoreContext<CustomDbContext>("Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True", "Microsoft.Data.Sqlite");
    }

    internal class CustomDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public CustomDbContext(DbContextOptions<CustomDbContext> options)
            : base(options)
        {
        }
    }
}
