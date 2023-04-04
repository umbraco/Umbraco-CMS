using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
public class EfCoreScopeInfrastructureScopeTests : UmbracoIntegrationTest
{
    private IEfCoreScopeProvider<UmbracoEFContext> EfCoreScopeProvider =>
        GetRequiredService<IEfCoreScopeProvider<UmbracoEFContext>>();

    private IScopeProvider InfrastructureScopeProvider =>
        GetRequiredService<IScopeProvider>();

    private EFCoreScopeAccessor<UmbracoEFContext> EfCoreScopeAccessor => (EFCoreScopeAccessor<UmbracoEFContext>)GetRequiredService<IEFCoreScopeAccessor<UmbracoEFContext>>();

    private IScopeAccessor InfrastructureScopeAccessor => GetRequiredService<IScopeAccessor>();

    [Test]
    public void CanCreateNestedInfrastructureScope()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        using (IEfCoreScope<UmbracoEFContext> scope = EfCoreScopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<EfCoreScope<UmbracoEFContext>>(scope);
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
        using (IEfCoreScope<UmbracoEFContext> parentScope = EfCoreScopeProvider.CreateScope())
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
        using (IEfCoreScope<UmbracoEFContext> scope = EfCoreScopeProvider.CreateScope())
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

            using (IEfCoreScope<UmbracoEFContext> scope = EfCoreScopeProvider.CreateScope())
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
        using (IEfCoreScope<UmbracoEFContext> scope = EfCoreScopeProvider.CreateScope())
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
        using (IEfCoreScope<UmbracoEFContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });
            scope.Complete();
        }

        using (IEfCoreScope<UmbracoEFContext> parentScope = EfCoreScopeProvider.CreateScope())
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
        using (IEfCoreScope<UmbracoEFContext> scope = EfCoreScopeProvider.CreateScope())
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
        using (IEfCoreScope<UmbracoEFContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });

            scope.Complete();
        }

        using (IScope parentScope = InfrastructureScopeProvider.CreateScope())
        {
            using (IEfCoreScope<UmbracoEFContext> scope = EfCoreScopeProvider.CreateScope())
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
        using (IEfCoreScope<UmbracoEFContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNull(result);
            });
        }
    }
}
