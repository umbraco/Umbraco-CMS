using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Persistence.Sqlite.Interceptors;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore;

[TestFixture]
[Timeout(60000)]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class EfCoreLockTests : UmbracoIntegrationTest
{

    private IEfCoreScopeProvider EfCoreScopeProvider =>
        GetRequiredService<IEfCoreScopeProvider>();

    private EFCoreScopeAccessor EfCoreScopeAccessor => (EFCoreScopeAccessor)GetRequiredService<IEFCoreScopeAccessor>();

    [SetUp]
    protected void SetUp()
    {
        // create a few lock objects
        using (var scope = EfCoreScopeProvider.CreateScope())
        {
            scope.ExecuteWithContextAsync<Task>(async database =>
            {
                database.UmbracoLocks.Add(new UmbracoLock {Id = 1, Name = "Lock.1"});
                database.UmbracoLocks.Add(new UmbracoLock {Id = 1, Name = "Lock.2"});
                database.UmbracoLocks.Add(new UmbracoLock {Id = 1, Name = "Lock.3"});

            });

            scope.Complete();
        }
    }

    protected override void ConfigureTestServices(IServiceCollection services) =>
        // SQLite + retry policy makes tests fail, we retry before throwing distributed locking timeout.
        services.RemoveAll(x => x.ImplementationType == typeof(SqliteAddRetryPolicyInterceptor));

    [Test]
    public void SingleReadLockTest()
    {
        using (var scope = EfCoreScopeProvider.CreateScope())
        {
            scope.EagerReadLock(Constants.Locks.Servers);
            scope.Complete();
        }
    }
}
