using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class AuditTriggerAccessorTests
{
    [Test]
    public void Current_Returns_Null_When_No_Scope_Context()
    {
        var scopeProviderMock = new Mock<ICoreScopeProvider>();
        scopeProviderMock.Setup(x => x.Context).Returns((IScopeContext?)null);

        var accessor = new AuditTriggerAccessor(scopeProviderMock.Object);

        Assert.IsNull(accessor.Current);
    }

    [Test]
    public void Current_Returns_Null_When_No_Trigger_Enlisted()
    {
        var scopeContextMock = new Mock<IScopeContext>();
        scopeContextMock
            .Setup(x => x.GetEnlisted<AuditTrigger>(It.IsAny<string>()))
            .Returns((AuditTrigger?)null);

        var scopeProviderMock = new Mock<ICoreScopeProvider>();
        scopeProviderMock.Setup(x => x.Context).Returns(scopeContextMock.Object);

        var accessor = new AuditTriggerAccessor(scopeProviderMock.Object);

        Assert.IsNull(accessor.Current);
    }

    [Test]
    public void Set_Enlists_Trigger_On_Scope_Context()
    {
        var trigger = new AuditTrigger("Core", "ScheduledPublish");
        AuditTrigger? enlistedTrigger = null;

        var scopeContextMock = new Mock<IScopeContext>();
        scopeContextMock
            .Setup(x => x.Enlist(
                It.IsAny<string>(),
                It.IsAny<Func<AuditTrigger>>(),
                It.IsAny<Action<bool, AuditTrigger?>?>(),
                It.IsAny<int>()))
            .Callback<string, Func<AuditTrigger>, Action<bool, AuditTrigger?>?, int>(
                (key, creator, action, priority) => enlistedTrigger = creator());

        var scopeProviderMock = new Mock<ICoreScopeProvider>();
        scopeProviderMock.Setup(x => x.Context).Returns(scopeContextMock.Object);

        var accessor = new AuditTriggerAccessor(scopeProviderMock.Object);
        accessor.Set(trigger);

        Assert.IsNotNull(enlistedTrigger);
        Assert.AreEqual("Core", enlistedTrigger!.Source);
        Assert.AreEqual("ScheduledPublish", enlistedTrigger.Operation);
    }

    [Test]
    public void Set_Does_Not_Throw_When_No_Scope_Context()
    {
        var scopeProviderMock = new Mock<ICoreScopeProvider>();
        scopeProviderMock.Setup(x => x.Context).Returns((IScopeContext?)null);

        var accessor = new AuditTriggerAccessor(scopeProviderMock.Object);

        Assert.DoesNotThrow(() => accessor.Set(new AuditTrigger("Core", "Save")));
    }

    [Test]
    public void Current_Returns_Trigger_After_Set()
    {
        var trigger = new AuditTrigger("Umbraco.Workflow", "Approve");

        var scopeContextMock = new Mock<IScopeContext>();
        scopeContextMock
            .Setup(x => x.GetEnlisted<AuditTrigger>(It.IsAny<string>()))
            .Returns(trigger);

        var scopeProviderMock = new Mock<ICoreScopeProvider>();
        scopeProviderMock.Setup(x => x.Context).Returns(scopeContextMock.Object);

        var accessor = new AuditTriggerAccessor(scopeProviderMock.Object);

        Assert.AreEqual(trigger, accessor.Current);
        Assert.AreEqual("Umbraco.Workflow", accessor.Current!.Source);
        Assert.AreEqual("Approve", accessor.Current.Operation);
    }

    [Test]
    public void First_Writer_Wins_Via_ScopeContext_Enlist_Semantics()
    {
        // ScopeContext.Enlist returns the first enlisted object for a given key,
        // ignoring subsequent calls. We verify the accessor delegates to Enlist
        // (not direct set), so first-writer-wins is guaranteed by ScopeContext.
        var firstTrigger = new AuditTrigger("Core", "ScheduledPublish");
        var secondTrigger = new AuditTrigger("Umbraco.Workflow", "Approve");

        var scopeContext = new ScopeContext();

        var scopeProviderMock = new Mock<ICoreScopeProvider>();
        scopeProviderMock.Setup(x => x.Context).Returns(scopeContext);

        var accessor = new AuditTriggerAccessor(scopeProviderMock.Object);

        accessor.Set(firstTrigger);
        accessor.Set(secondTrigger);

        Assert.AreEqual(firstTrigger, accessor.Current);
        Assert.AreEqual("Core", accessor.Current!.Source);
        Assert.AreEqual("ScheduledPublish", accessor.Current.Operation);
    }
}
