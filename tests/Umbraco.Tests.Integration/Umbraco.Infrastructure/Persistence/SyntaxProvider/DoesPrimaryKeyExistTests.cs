using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.SyntaxProvider;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
internal sealed class DoesPrimaryKeyExistTests : UmbracoIntegrationTest
{
    [Test]
    public void Can_Detect_Existing_Primary_Key()
    {
        var factory = GetRequiredService<IUmbracoDatabaseFactory>();
        var database = factory.CreateDatabase();

        var builder = GetBuilder(database);
        builder.Table("foo")
            .WithColumn("id").AsInt32().PrimaryKey("PK_foo")
            .Do();

        var result = database.SqlContext.SqlSyntax.DoesPrimaryKeyExist(database, "foo", "PK_foo");

        Assert.IsTrue(result);
    }

    [Test]
    public void Cannot_Detect_Non_Existing_Primary_Key()
    {
        var factory = GetRequiredService<IUmbracoDatabaseFactory>();
        var database = factory.CreateDatabase();

        var builder = GetBuilder(database);
        builder.Table("foo")
            .WithColumn("id").AsInt32().PrimaryKey("PK_foo")
            .Do();

        var result = database.SqlContext.SqlSyntax.DoesPrimaryKeyExist(database, "foo", "PK_does_not_exist");

        Assert.IsFalse(result);
    }

    private CreateBuilder GetBuilder(IUmbracoDatabase db)
    {
        var logger = GetRequiredService<ILogger<MigrationContext>>();
        var ctx = new MigrationContext(new TestPlan(), db, logger);
        return new CreateBuilder(ctx);
    }

    private class TestPlan : MigrationPlan
    {
        public TestPlan()
            : base("test-plan")
        {
        }
    }
}