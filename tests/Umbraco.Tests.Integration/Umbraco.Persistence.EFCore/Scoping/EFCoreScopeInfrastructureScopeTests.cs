using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
internal sealed class EFCoreScopeInfrastructureScopeTests : UmbracoIntegrationTest
{
    private IEFCoreScopeProvider<TestUmbracoDbContext> EFCoreScopeProvider =>
        GetRequiredService<IEFCoreScopeProvider<TestUmbracoDbContext>>();

    private IScopeProvider InfrastructureScopeProvider =>
        GetRequiredService<IScopeProvider>();

    private EFCoreScopeAccessor<TestUmbracoDbContext> EFCoreScopeAccessor => (EFCoreScopeAccessor<TestUmbracoDbContext>)GetRequiredService<IEFCoreScopeAccessor<TestUmbracoDbContext>>();

    private IScopeAccessor InfrastructureScopeAccessor => GetRequiredService<IScopeAccessor>();

    [Test]
    public void CanCreateNestedInfrastructureScope()
    {
        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            Assert.That(scope, Is.InstanceOf<EFCoreScope<TestUmbracoDbContext>>());
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);
            Assert.That(InfrastructureScopeAccessor.AmbientScope, Is.Not.Null);
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.SameAs(scope));
            using (var infrastructureScope = InfrastructureScopeProvider.CreateScope())
            {
                Assert.That(InfrastructureScopeAccessor.AmbientScope, Is.SameAs(infrastructureScope));
            }

            Assert.That(InfrastructureScopeAccessor.AmbientScope, Is.Not.Null);
        }

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
        Assert.That(InfrastructureScopeAccessor.AmbientScope, Is.Null);
    }

    [Test]
    public async Task? TransactionWithEFCoreScopeAsParent()
    {
        using (IEFCoreScope<TestUmbracoDbContext> parentScope = EFCoreScopeProvider.CreateScope())
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
                Assert.That(n, Is.EqualTo("a"));
                childScope.Complete();
            }

            await parentScope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.That(result, Is.EqualTo("a"));
            });


            parentScope.Complete();
        }

        // Check that its not rolled back
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.That(result, Is.Not.Null);
            });
        }
    }

    [Test]
    public async Task? TransactionWithInfrastructureScopeAsParent()
    {
        using (IScope parentScope = InfrastructureScopeProvider.CreateScope())
        {
            parentScope.Database.Execute("CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");

            using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
            {
                await scope.ExecuteWithContextAsync<Task>(async database =>
                {
                    await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");

                    string? result =
                        await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                    Assert.That(result, Is.EqualTo("a"));
                });

                scope.Complete();
            }

            parentScope.Complete();
        }

        // Check that its not rolled back
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.That(result, Is.Not.Null);
            });
        }
    }

    [Test]
    public async Task EFCoreAsParent_DontCompleteWhenChildScopeDoesNotComplete()
    {
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });
            scope.Complete();
        }

        using (IEFCoreScope<TestUmbracoDbContext> parentScope = EFCoreScopeProvider.CreateScope())
        {
            using (IScope scope = InfrastructureScopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.That(n, Is.EqualTo("a"));
            }

            await parentScope.ExecuteWithContextAsync<Task>(async database =>
            {
                // Should still be in transaction and not rolled back yet
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.That(result, Is.EqualTo("a"));
            });

            parentScope.Complete();
        }

        // Check that its rolled back
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                // Should still be in transaction and not rolled back yet
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.That(result, Is.Null);
            });
        }
    }

    [Test]
    public async Task InfrastructureScopeAsParent_DontCompleteWhenChildScopeDoesNotComplete()
    {
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });

            scope.Complete();
        }

        using (IScope parentScope = InfrastructureScopeProvider.CreateScope())
        {
            using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
            {
                await scope.ExecuteWithContextAsync<Task>(async database =>
                {
                    await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");

                    string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                    Assert.That(result, Is.EqualTo("a"));
                });

                string n = parentScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.That(n, Is.EqualTo("a"));
            }

            parentScope.Complete();
        }

        // Check that its rolled back
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.That(result, Is.Null);
            });
        }
    }
}
