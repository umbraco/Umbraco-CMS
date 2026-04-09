using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.DistributedLocking;

/// <summary>
/// Tests for <see cref="EFCoreScopeAccessor{TDbContext}.HasBridgedAmbientScope"/>.
/// </summary>
[TestFixture]
public class EFCoreScopeAccessorTests
{
    private static EFCoreScopeAccessor<UmbracoDbContext> CreateAccessor(
        IAmbientEFCoreScopeStack<UmbracoDbContext> efScopeStack,
        IAmbientScopeStack npocoScopeStack,
        Lazy<IEFCoreScopeProvider<UmbracoDbContext>>? efScopeProvider = null)
    {
        efScopeProvider ??= new Lazy<IEFCoreScopeProvider<UmbracoDbContext>>(
            () => throw new InvalidOperationException("EFCoreScopeProvider should not be accessed in this test."));

        return new EFCoreScopeAccessor<UmbracoDbContext>(efScopeStack, npocoScopeStack, efScopeProvider);
    }

    [Test]
    public void HasBridgedAmbientScope_NoScopeOnStack_ReturnsTrue()
    {
        var efScopeStack = new Mock<IAmbientEFCoreScopeStack<UmbracoDbContext>>();
        efScopeStack.Setup(x => x.AmbientScope).Returns((IEfCoreScope<UmbracoDbContext>?)null);

        var accessor = CreateAccessor(efScopeStack.Object, Mock.Of<IAmbientScopeStack>());

        Assert.That(accessor.HasBridgedAmbientScope, Is.True);
    }

    [Test]
    public void HasBridgedAmbientScope_NonEFCoreScopeOnStack_ReturnsTrue()
    {
        // A mock of IEfCoreScope<T> is not an EFCoreScope<T> concrete type,
        // so the type pattern match `is EFCoreScope<T> { IsBridgeScope: false }` fails.
        // This covers the bridge-scope-on-stack case (bridge scopes are EFCoreScope<T>
        // instances with IsBridgeScope: true, but in terms of the type check they would
        // also be EFCoreScope<T> — the mock here acts as a stand-in for the "not a real
        // non-bridge scope" scenario).
        var efScopeStack = new Mock<IAmbientEFCoreScopeStack<UmbracoDbContext>>();
        efScopeStack.Setup(x => x.AmbientScope).Returns(Mock.Of<IEfCoreScope<UmbracoDbContext>>());

        var accessor = CreateAccessor(efScopeStack.Object, Mock.Of<IAmbientScopeStack>());

        Assert.That(accessor.HasBridgedAmbientScope, Is.True);
    }

    [Test]
    public void HasBridgedAmbientScope_WhenNpocoScopeIsActive_DoesNotTriggerBridgeScopeCreation()
    {
        // Regression test: the old Enabled check used `AmbientScope is not null`, which
        // auto-creates a bridge scope when an NPoco scope is active (via GetOrCreateAmbientScope).
        // HasBridgedAmbientScope must read the raw stack directly without any side effects.
        //
        // We verify this by:
        //   - Raw EF scope stack is empty (AmbientScope returns null)
        //   - An NPoco scope IS active (would normally trigger bridge scope creation)
        //   - The EFCoreScopeProvider Lazy is rigged to throw if accessed
        //   - HasBridgedAmbientScope must return true without throwing

        var efScopeStack = new Mock<IAmbientEFCoreScopeStack<UmbracoDbContext>>();
        efScopeStack.Setup(x => x.AmbientScope).Returns((IEfCoreScope<UmbracoDbContext>?)null);

        var npocoScopeStack = new Mock<IAmbientScopeStack>();
        npocoScopeStack.Setup(x => x.AmbientScope).Returns(Mock.Of<IScope>());

        var providerThatShouldNotBeAccessed = new Lazy<IEFCoreScopeProvider<UmbracoDbContext>>(
            () => throw new InvalidOperationException("Accessing the EFCoreScopeProvider would create a bridge scope — this is the bug we fixed."));

        var accessor = CreateAccessor(efScopeStack.Object, npocoScopeStack.Object, providerThatShouldNotBeAccessed);

        // Must not throw (if it accessed the provider, it would throw and fail the test).
        Assert.That(accessor.HasBridgedAmbientScope, Is.True);
    }
}
