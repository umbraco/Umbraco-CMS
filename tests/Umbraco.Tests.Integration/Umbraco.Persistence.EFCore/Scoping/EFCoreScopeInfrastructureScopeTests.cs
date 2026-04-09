using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Extensions;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
internal sealed class EFCoreScopeInfrastructureScopeTests : UmbracoIntegrationTest
{
    private IEFCoreScopeProvider<TestUmbracoDbContext> EfCoreScopeProvider =>
        GetRequiredService<IEFCoreScopeProvider<TestUmbracoDbContext>>();

    private IScopeProvider InfrastructureScopeProvider =>
        GetRequiredService<IScopeProvider>();

    private EFCoreScopeAccessor<TestUmbracoDbContext> EfCoreScopeAccessor => (EFCoreScopeAccessor<TestUmbracoDbContext>)GetRequiredService<IEFCoreScopeAccessor<TestUmbracoDbContext>>();

    private IScopeAccessor InfrastructureScopeAccessor => GetRequiredService<IScopeAccessor>();

    [Test]
    public void CanCreateNestedInfrastructureScope()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(scope);
            Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
            Assert.IsNotNull(InfrastructureScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EfCoreScopeAccessor.AmbientScope);
            using (var infrastructureScope = InfrastructureScopeProvider.CreateScope())
            {
                Assert.AreSame(infrastructureScope, InfrastructureScopeAccessor.AmbientScope);
            }

            Assert.IsNotNull(InfrastructureScopeAccessor.AmbientScope);
        }

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        Assert.IsNull(InfrastructureScopeAccessor.AmbientScope);
    }

    [Test]
    public async Task? TransactionWithEfCoreScopeAsParent()
    {
        using (IEfCoreScope<TestUmbracoDbContext> parentScope = EfCoreScopeProvider.CreateScope())
        {
            await parentScope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });

            // This should be using same transaction, so insert data into table we're creating
            using (IScope childScope = InfrastructureScopeProvider.CreateScope())
            {
                childScope.Database.Execute("INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>(
                    "SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
                childScope.Complete();
            }

            await parentScope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", result);
            });


            parentScope.Complete();
        }

        // Check that its not rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNotNull(result);
            });
        }
    }

    [Test]
    public async Task? TransactionWithInfrastructureScopeAsParent()
    {
        using (IScope parentScope = InfrastructureScopeProvider.CreateScope())
        {
            parentScope.Database.Execute("CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");

            using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
            {
                await scope.ExecuteWithContextAsync<Task>(async database =>
                {
                    await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");

                    string? result =
                        await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                    Assert.AreEqual("a", result);
                });

                scope.Complete();
            }

            parentScope.Complete();
        }

        // Check that its not rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNotNull(result);
            });
        }
    }

    [Test]
    public async Task EFCoreAsParent_DontCompleteWhenChildScopeDoesNotComplete()
    {
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });
            scope.Complete();
        }

        using (IEfCoreScope<TestUmbracoDbContext> parentScope = EfCoreScopeProvider.CreateScope())
        {
            using (IScope scope = InfrastructureScopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
            }

            await parentScope.ExecuteWithContextAsync<Task>(async database =>
            {
                // Should still be in transaction and not rolled back yet
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", result);
            });

            parentScope.Complete();
        }

        // Check that its rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                // Should still be in transaction and not rolled back yet
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNull(result);
            });
        }
    }

    [Test]
    public async Task InfrastructureScopeAsParent_DontCompleteWhenChildScopeDoesNotComplete()
    {
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });

            scope.Complete();
        }

        using (IScope parentScope = InfrastructureScopeProvider.CreateScope())
        {
            using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
            {
                await scope.ExecuteWithContextAsync<Task>(async database =>
                {
                    await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");

                    string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                    Assert.AreEqual("a", result);
                });

                string n = parentScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
            }

            parentScope.Complete();
        }

        // Check that its rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNull(result);
            });
        }
    }

    /// <summary>
    /// Simulates an EF Core repository being accessed during an NPoco scope.
    /// The EFCoreScopeAccessor auto-creates a bridge scope so both ORMs share the same transaction.
    /// Verifies bidirectional data visibility and that the transaction commits when the NPoco scope completes.
    /// </summary>
    [Test]
    public async Task BridgedScope_NPocoScopeAccessesEfCoreRepository_SharesTransaction()
    {
        using (IScope npocoScope = InfrastructureScopeProvider.CreateScope())
        {
            npocoScope.Database.Execute("CREATE TABLE tmp_bridge (id INT, name NVARCHAR(64))");
            npocoScope.Database.Execute("INSERT INTO tmp_bridge (id, name) VALUES (1, 'npoco_row')");

            // Simulate an EF Core repository accessing the ambient scope via the accessor.
            // Since only an NPoco scope is active, the accessor auto-creates a bridge scope.
            var bridgedScope = EfCoreScopeAccessor.AmbientScope;
            Assert.IsNotNull(bridgedScope);
            Assert.IsTrue(((EFCoreScope<TestUmbracoDbContext>)bridgedScope!).IsBridgeScope);

            // EF Core can see NPoco's uncommitted writes (same transaction)
            await bridgedScope.ExecuteWithContextAsync<Task>(async db =>
            {
                string? npocoData = await db.Database.ExecuteScalarAsync<string>(
                    "SELECT name FROM tmp_bridge WHERE id=1");
                Assert.AreEqual("npoco_row", npocoData);

                // EF Core writes in the same transaction
                await db.Database.ExecuteSqlAsync(
                    $"INSERT INTO tmp_bridge (id, name) VALUES (2, 'efcore_row')");
            });

            // NPoco can see EF Core's writes (same transaction)
            string efCoreData = npocoScope.Database.ExecuteScalar<string>(
                "SELECT name FROM tmp_bridge WHERE id=2");
            Assert.AreEqual("efcore_row", efCoreData);



            npocoScope.Complete();
        }

        // Both rows committed
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async db =>
            {
                string? row1 = await db.Database.ExecuteScalarAsync<string>(
                    "SELECT name FROM tmp_bridge WHERE id=1");
                string? row2 = await db.Database.ExecuteScalarAsync<string>(
                    "SELECT name FROM tmp_bridge WHERE id=2");
                Assert.AreEqual("npoco_row", row1);
                Assert.AreEqual("efcore_row", row2);
            });
            scope.Complete();
        }
    }

    /// <summary>
    /// Verifies that when an NPoco scope is NOT completed, the bridge scope's EF Core
    /// writes are also rolled back since they share the same underlying transaction.
    /// </summary>
    [Test]
    public async Task BridgedScope_NPocoScopeNotCompleted_EfCoreWritesRolledBack()
    {
        // Setup: create table in a committed scope
        using (IScope setup = InfrastructureScopeProvider.CreateScope())
        {
            setup.Database.Execute("CREATE TABLE tmp_bridge2 (id INT, name NVARCHAR(64))");
            setup.Complete();
        }

        // Insert via bridge scope, but do NOT complete the NPoco parent
        using (IScope npocoScope = InfrastructureScopeProvider.CreateScope())
        {
            // Simulate EF Core repository access — triggers bridge scope creation
            IEfCoreScope<TestUmbracoDbContext>? bridgeScope = EfCoreScopeAccessor.AmbientScope;
            Assert.IsNotNull(bridgeScope);

            await bridgeScope!.ExecuteWithContextAsync<Task>(async db =>
            {
                await db.Database.ExecuteSqlAsync(
                    $"INSERT INTO tmp_bridge2 (id, name) VALUES (1, 'should_rollback')");

                // Data is visible within the transaction
                string? result = await db.Database.ExecuteScalarAsync<string>(
                    "SELECT name FROM tmp_bridge2 WHERE id=1");
                Assert.AreEqual("should_rollback", result);
            });

            // Do NOT call npocoScope.Complete() — transaction should roll back
        }

        // Verify rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async db =>
            {
                string? result = await db.Database.ExecuteScalarAsync<string>(
                    "SELECT name FROM tmp_bridge2 WHERE id=1");
                Assert.IsNull(result);
            });
        }
    }
}
