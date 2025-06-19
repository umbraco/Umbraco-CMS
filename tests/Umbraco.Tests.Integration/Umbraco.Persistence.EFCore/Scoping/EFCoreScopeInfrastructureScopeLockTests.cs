using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class EFCoreScopeInfrastructureScopeLockTests : UmbracoIntegrationTest
{
    private IEFCoreScopeProvider<TestUmbracoDbContext> EfCoreScopeProvider =>
        GetRequiredService<IEFCoreScopeProvider<TestUmbracoDbContext>>();

    private IScopeProvider InfrastructureScopeProvider =>
        GetRequiredService<IScopeProvider>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.AddNotificationHandler<TestSendNotification, TestSendNotificationHandler>();
        builder.AddNotificationHandler<TestDoNotSendNotification, TestDoNotSendNotificationHandler>();
    }

    [Test]
    public async Task ScopesCanShareNonEagerLocks()
    {
        using IEfCoreScope<TestUmbracoDbContext> parentScope = EfCoreScopeProvider.CreateScope();
        await parentScope.ExecuteWithContextAsync<Task>(async database =>
        {
            parentScope.Locks.WriteLock(parentScope.InstanceId, Constants.Locks.Servers);
            await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
        });

        using (var childScope = InfrastructureScopeProvider.CreateScope())
        {
            childScope.Locks.WriteLock(childScope.InstanceId, Constants.Locks.Servers);
            string n = childScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
            Assert.AreEqual("a", n);
            childScope.Complete();
        }

        parentScope.Complete();
    }

    [Test]
    public async Task ScopesCanShareEagerLocks()
    {
        using IEfCoreScope<TestUmbracoDbContext> parentScope = EfCoreScopeProvider.CreateScope();
        await parentScope.ExecuteWithContextAsync<Task>(async database =>
        {
            parentScope.Locks.EagerWriteLock(parentScope.InstanceId, Constants.Locks.Servers);
            await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
        });

        using (var childScope = InfrastructureScopeProvider.CreateScope())
        {
            childScope.Locks.EagerWriteLock(childScope.InstanceId, Constants.Locks.Servers);
            string n = childScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
            Assert.AreEqual("a", n);
            childScope.Complete();
        }

        parentScope.Complete();
    }

    [Test]
    public void EFCoreScopeAsParent_Child_Scope_Can_Send_Notification()
    {
        var currentAssertCount = TestContext.CurrentContext.AssertCount;
        using (var scope = EfCoreScopeProvider.CreateScope())
        {
            using (var childScope = InfrastructureScopeProvider.CreateScope())
            {
                var savingNotification = new TestSendNotification();
                childScope.Notifications.Publish(savingNotification);
                childScope.Complete();
            }

            // Assert notifications arent send on completion of scope
            Assert.AreEqual(currentAssertCount, TestContext.CurrentContext.AssertCount);

            scope.Complete();
        }

        Assert.AreEqual(currentAssertCount + 2, TestContext.CurrentContext.AssertCount);
    }

    [Test]
    public void InfrastructureScopeAsParent_Child_Scope_Can_Send_Notification()
    {
        var currentAssertCount = TestContext.CurrentContext.AssertCount;
        using (var scope = InfrastructureScopeProvider.CreateScope())
        {
            using (var childScope = EfCoreScopeProvider.CreateScope())
            {
                var savingNotification = new TestSendNotification();
                childScope.Notifications.Publish(savingNotification);
                childScope.Complete();
            }

            // Assert notifications arent send on completion of scope
            Assert.AreEqual(currentAssertCount, TestContext.CurrentContext.AssertCount);

            scope.Complete();
        }

        Assert.AreEqual(currentAssertCount + 2, TestContext.CurrentContext.AssertCount);
    }

    private class TestSendNotification : INotification
    {
    }

    private class TestDoNotSendNotification : INotification
    {
    }

    private class TestSendNotificationHandler : INotificationHandler<TestSendNotification>
    {
        public void Handle(TestSendNotification notification)
            => Assert.IsNotNull(notification);
    }

    private class TestDoNotSendNotificationHandler : INotificationHandler<TestDoNotSendNotification>
    {
        public void Handle(TestDoNotSendNotification notification)
            => Assert.Fail("Notification was sent");
    }
}
