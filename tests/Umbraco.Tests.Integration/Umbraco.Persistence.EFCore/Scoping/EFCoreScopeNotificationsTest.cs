using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
internal sealed class EFCoreScopeNotificationsTest : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.AddNotificationHandler<TestSendNotification, TestSendNotificationHandler>();
        builder.AddNotificationHandler<TestDoNotSendNotification, TestDoNotSendNotificationHandler>();
    }

    private IEFCoreScopeProvider<TestUmbracoDbContext> EfCoreScopeProvider => GetRequiredService<IEFCoreScopeProvider<TestUmbracoDbContext>>();

    [Test]
    public void Scope_Can_Send_Notification()
    {
        // We do asserts in the setup of Umbraco, therefore get the
        // current number of asserts right how, and assert later that this
        // has only gone up by 1
        var initialAssertCount = TestContext.CurrentContext.AssertCount;

        using (var scope = EfCoreScopeProvider.CreateScope())
        {
            var savingNotification = new TestSendNotification();
            scope.Notifications.Publish(savingNotification);
            scope.Complete();
        }

        Assert.AreEqual(initialAssertCount + 1, TestContext.CurrentContext.AssertCount);
    }

    [Test]
    public void Child_Scope_Can_Send_Notification()
    {
        var initialAssertCount = TestContext.CurrentContext.AssertCount;
        using (var scope = EfCoreScopeProvider.CreateScope())
        {
            using (var childScope = EfCoreScopeProvider.CreateScope())
            {
                var savingNotification = new TestSendNotification();
                childScope.Notifications.Publish(savingNotification);
                childScope.Complete();
            }

            scope.Complete();
        }

        Assert.AreEqual(initialAssertCount + 1, TestContext.CurrentContext.AssertCount);
    }

    [Test]
    public void Scope_Does_Not_Send_Notification_When_Not_Completed()
    {
        using var scope = EfCoreScopeProvider.CreateScope();

        var savingNotification = new TestDoNotSendNotification();
        scope.Notifications.Publish(savingNotification);
    }

    [Test]
    public void Scope_Does_Not_Send_Notification_When_Suppressing()
    {
        using var scope = EfCoreScopeProvider.CreateScope();
        scope.Notifications.Suppress();
        var savingNotification = new TestDoNotSendNotification();
        scope.Notifications.Publish(savingNotification);
        scope.Complete();
    }

    [Test]
    public void Child_Scope_Cannot_Send_Suppressed_Notification()
    {
        using var scope = EfCoreScopeProvider.CreateScope();

        using (var childScope = EfCoreScopeProvider.CreateScope())
        {
            childScope.Notifications.Suppress();
            var savingNotification = new TestDoNotSendNotification();
            childScope.Notifications.Publish(savingNotification);
        }

        scope.Complete();
    }

    [Test]
    public void Parent_Scope_Can_Send_Notification_Before_Child_Suppressing()
    {
        var initialAssertCount = TestContext.CurrentContext.AssertCount;

        using (var scope = EfCoreScopeProvider.CreateScope())
        {
            var savingParentNotification = new TestSendNotification();
            scope.Notifications.Publish(savingParentNotification);
            using (var childScope = EfCoreScopeProvider.CreateScope())
            {
                childScope.Notifications.Suppress();
                var savingNotification = new TestDoNotSendNotification();
                childScope.Notifications.Publish(savingNotification);
                childScope.Complete();
            }

            scope.Complete();
        }

        Assert.AreEqual(initialAssertCount + 1, TestContext.CurrentContext.AssertCount);
    }

    [Test]
    public void Parent_Scope_Can_Send_Notification_After_Child_Suppressing()
    {
        var initialAssertCount = TestContext.CurrentContext.AssertCount;


        using (var scope = EfCoreScopeProvider.CreateScope())
        {
            using (var childScope = EfCoreScopeProvider.CreateScope())
            {
                using (childScope.Notifications.Suppress())
                {
                    var savingNotification = new TestDoNotSendNotification();
                    childScope.Notifications.Publish(savingNotification);
                    childScope.Complete();
                }
            }

            var savingParentNotificationTwo = new TestSendNotification();
            scope.Notifications.Publish(savingParentNotificationTwo);

            scope.Complete();
        }

        Assert.AreEqual(initialAssertCount + 1, TestContext.CurrentContext.AssertCount);
    }

    [Test]
    public void Scope_Can_Send_Notification_After_Suppression_Disposed()
    {
        var initialAssertCount = TestContext.CurrentContext.AssertCount;

        using (var scope = EfCoreScopeProvider.CreateScope())
        {
            using (scope.Notifications.Suppress())
            {
                var savingNotification = new TestDoNotSendNotification();
                scope.Notifications.Publish(savingNotification);
            }

            var savingParentNotificationTwo = new TestSendNotification();
            scope.Notifications.Publish(savingParentNotificationTwo);

            scope.Complete();
        }

        Assert.AreEqual(initialAssertCount + 1, TestContext.CurrentContext.AssertCount);
    }

    [Test]
    public void Child_Scope_Does_Not_Send_Notification_When_Parent_Suppressing()
    {
        using var scope = EfCoreScopeProvider.CreateScope();
        scope.Notifications.Suppress();

        using (var childScope = EfCoreScopeProvider.CreateScope())
        {
            var savingNotification = new TestDoNotSendNotification();
            childScope.Notifications.Publish(savingNotification);
            childScope.Complete();
        }

        scope.Complete();
    }

    [Test]
    public void Cant_Suppress_Notifactions_On_Child_When_Parent_Suppressing()
    {
        using var parentScope = EfCoreScopeProvider.CreateScope();
        using var parentSuppressed = parentScope.Notifications.Suppress();
        using var childScope = EfCoreScopeProvider.CreateScope();
        Assert.Throws<InvalidOperationException>(() => childScope.Notifications.Suppress());
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
