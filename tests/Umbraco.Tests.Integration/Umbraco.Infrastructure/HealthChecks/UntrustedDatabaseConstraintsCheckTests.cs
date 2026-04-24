using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Infrastructure.HealthChecks.Checks.Data;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.HealthChecks;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class UntrustedDatabaseConstraintsCheckTests : UmbracoIntegrationTest
{
    private const string TestConstraintName = "FK_umbracoRelation_umbracoNode";

    private UntrustedDatabaseConstraintsCheck CreateSut()
        => new(Services.GetRequiredService<IUmbracoDatabaseFactory>());

    [Test]
    public async Task FreshDatabase_ReportsSuccessOnSqlServer()
    {
        if (BaseTestDatabase.IsSqlite())
        {
            Assert.Ignore("Untrusted constraints are a SQL Server concept and do not apply to SQLite.");
            return;
        }

        HealthCheckStatus status = (await CreateSut().GetStatusAsync()).Single();

        Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Success));
    }

    [Test]
    public async Task UntrustedConstraint_ReportsWarningAndIncludesConstraintName()
    {
        if (BaseTestDatabase.IsSqlite())
        {
            Assert.Ignore("Untrusted constraints are a SQL Server concept and do not apply to SQLite.");
            return;
        }

        MakeConstraintUntrusted(TestConstraintName);

        try
        {
            HealthCheckStatus status = (await CreateSut().GetStatusAsync()).Single();

            Assert.Multiple(() =>
            {
                Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Warning));
                Assert.That(status.Description, Does.Contain(TestConstraintName));
                Assert.That(status.ReadMoreLink, Is.Not.Null.And.Not.Empty);
            });
        }
        finally
        {
            RetrustConstraint(TestConstraintName);
        }
    }

    [Test]
    public async Task SqliteDatabase_ReportsInfoShortCircuit()
    {
        if (BaseTestDatabase.IsSqlite() is false)
        {
            Assert.Ignore("This test verifies the SQLite short-circuit path.");
            return;
        }

        HealthCheckStatus status = (await CreateSut().GetStatusAsync()).Single();

        Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Info));
    }

    private void MakeConstraintUntrusted(string constraintName)
        => ExecuteNonQuery(
            $"ALTER TABLE [umbracoRelation] NOCHECK CONSTRAINT [{constraintName}];" +
            $"ALTER TABLE [umbracoRelation] WITH NOCHECK CHECK CONSTRAINT [{constraintName}];");

    private void RetrustConstraint(string constraintName)
        => ExecuteNonQuery($"ALTER TABLE [umbracoRelation] WITH CHECK CHECK CONSTRAINT [{constraintName}];");

    private void ExecuteNonQuery(string sql)
    {
        using IUmbracoDatabase db = Services.GetRequiredService<IUmbracoDatabaseFactory>().CreateDatabase();
        db.Execute(sql);
    }
}
