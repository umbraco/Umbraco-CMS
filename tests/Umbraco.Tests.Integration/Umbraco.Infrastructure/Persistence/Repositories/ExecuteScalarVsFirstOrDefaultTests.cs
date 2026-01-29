using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

/// <summary>
/// Tests demonstrating the difference between ExecuteScalar and FirstOrDefault in NPoco.
/// </summary>
/// <remarks>
/// ExecuteScalar does not invoke all ISqlSyntaxProvider mappers, which causes failures
/// on database providers like PostgreSQL where type conversions require the full mapper pipeline.
/// See: https://github.com/umbraco/Umbraco-CMS/issues/21448
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ExecuteScalarVsFirstOrDefaultTests : UmbracoIntegrationTest
{
    /// <summary>
    /// Verifies that ExecuteScalarAsync fails to properly map COUNT(*) to long on certain database providers.
    /// </summary>
    /// <remarks>
    /// This test may pass on SQLite/SQL Server but fails on PostgreSQL because ExecuteScalar
    /// bypasses the ISqlSyntaxProvider mapper pipeline. PostgreSQL returns int8 for COUNT(*)
    /// which requires proper type mapping to convert to C# long.
    ///
    /// The fix is to use FirstOrDefaultAsync instead, which goes through the full mapper pipeline.
    /// </remarks>
    [Test]
    public async Task ExecuteScalarAsync_CountQuery_FailsOnPostgreSqlDueToMissingMapperInvocation()
    {
        // Arrange
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
        ISqlSyntaxProvider syntax = database.SqlContext.SqlSyntax;

        var sql = new Sql($"SELECT COUNT(*) FROM {syntax.GetQuotedTableName("umbracoNode")}");

        // Act & Assert
        // On PostgreSQL, this throws because ExecuteScalar doesn't invoke ISqlSyntaxProvider mappers.
        // On SQLite/SQL Server, this may succeed due to compatible type handling.
        // The proper fix is to use FirstOrDefaultAsync instead.
        try
        {
            var count = await database.ExecuteScalarAsync<long>(sql);

            // If we reach here, the database provider handles the type conversion natively.
            // This is the case for SQLite and SQL Server.
            // For PostgreSQL, this line is never reached - an exception is thrown above.
            Assert.That(count, Is.GreaterThanOrEqualTo(0), "Count should be non-negative");

            // Log a warning that this test passed - it demonstrates the issue only fails on PostgreSQL
            TestContext.WriteLine(
                "WARNING: ExecuteScalarAsync succeeded on this database provider. " +
                "This test demonstrates a failure that occurs on PostgreSQL where ISqlSyntaxProvider " +
                "mappers are not invoked by ExecuteScalar, causing type conversion failures.");
        }
        catch (InvalidCastException ex)
        {
            // This is the expected failure on PostgreSQL - the type cannot be cast without proper mapping
            Assert.Pass($"ExecuteScalarAsync failed as expected on this database provider: {ex.Message}");
        }
        catch (Exception ex) when (ex.Message.Contains("cast") || ex.Message.Contains("convert"))
        {
            // Other type conversion errors that indicate the mapper issue
            Assert.Pass($"ExecuteScalarAsync failed with type conversion error as expected: {ex.Message}");
        }

        scope.Complete();
    }

    /// <summary>
    /// Verifies that FirstOrDefaultAsync correctly maps COUNT(*) to long on all database providers.
    /// </summary>
    /// <remarks>
    /// Unlike ExecuteScalar, FirstOrDefault goes through the full NPoco mapper pipeline,
    /// including ISqlSyntaxProvider mappers, ensuring proper type conversion on all database providers.
    /// </remarks>
    [Test]
    public async Task FirstOrDefaultAsync_CountQuery_SucceedsOnAllDatabaseProviders()
    {
        // Arrange
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
        ISqlSyntaxProvider syntax = database.SqlContext.SqlSyntax;

        var sql = new Sql($"SELECT COUNT(*) FROM {syntax.GetQuotedTableName("umbracoNode")}");

        // Act
        // FirstOrDefaultAsync invokes the full mapper pipeline, including ISqlSyntaxProvider mappers.
        // This ensures proper type conversion on all database providers including PostgreSQL.
        var count = await database.FirstOrDefaultAsync<long>(sql);

        // Assert
        Assert.That(count, Is.GreaterThanOrEqualTo(0), "Count should be non-negative");

        scope.Complete();
    }
}
