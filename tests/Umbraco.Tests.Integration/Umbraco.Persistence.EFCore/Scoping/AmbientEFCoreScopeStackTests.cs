using Moq;
using NUnit.Framework;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

/// <summary>
/// Shares the defect and the fix of <c>AmbientScopeStack</c>: an empty stack inherited from an ancestor execution
/// context is shared by every flow that branches off it, because AsyncLocal copy-on-write protects the reference
/// and not the instance. Lives here rather than in the unit test project, which does not reference EF Core.
/// </summary>
[TestFixture]
internal sealed class AmbientEFCoreScopeStackTests
{
    [Test]
    public async Task Push_IsolatesConcurrentFlowsThatInheritedAnEmptyStack()
    {
        var sut = new AmbientEFCoreScopeStack<TestUmbracoDbContext>();

        Drain(sut);

        sut.Push(Mock.Of<IEFCoreScope<TestUmbracoDbContext>>());
        sut.Pop();

        IEFCoreScope<TestUmbracoDbContext> scopeA = Mock.Of<IEFCoreScope<TestUmbracoDbContext>>();
        IEFCoreScope<TestUmbracoDbContext> scopeB = Mock.Of<IEFCoreScope<TestUmbracoDbContext>>();
        var pushedA = new TaskCompletionSource();
        var pushedB = new TaskCompletionSource();

        async Task<IEFCoreScope<TestUmbracoDbContext>?> PushThenReadAmbient(
            IEFCoreScope<TestUmbracoDbContext> scope,
            TaskCompletionSource pushed,
            Task otherPushed)
        {
            sut.Push(scope);
            pushed.SetResult();
            await otherPushed;
            return sut.AmbientScope;
        }

        IEFCoreScope<TestUmbracoDbContext>?[] ambient = await Task.WhenAll(
            Task.Run(() => PushThenReadAmbient(scopeA, pushedA, pushedB.Task)),
            Task.Run(() => PushThenReadAmbient(scopeB, pushedB, pushedA.Task)));

        Assert.Multiple(() =>
        {
            Assert.AreSame(scopeA, ambient[0], "Flow A observed another flow's ambient scope.");
            Assert.AreSame(scopeB, ambient[1], "Flow B observed another flow's ambient scope.");
        });
    }

    [Test]
    public void Push_KeepsNestedScopesOnTheSameStack()
    {
        var sut = new AmbientEFCoreScopeStack<TestUmbracoDbContext>();

        Drain(sut);

        IEFCoreScope<TestUmbracoDbContext> outer = Mock.Of<IEFCoreScope<TestUmbracoDbContext>>();
        IEFCoreScope<TestUmbracoDbContext> inner = Mock.Of<IEFCoreScope<TestUmbracoDbContext>>();

        sut.Push(outer);
        sut.Push(inner);

        Assert.Multiple(() =>
        {
            Assert.AreSame(inner, sut.AmbientScope);
            Assert.AreSame(inner, sut.Pop());
            Assert.AreSame(outer, sut.AmbientScope);
            Assert.AreSame(outer, sut.Pop());
            Assert.IsNull(sut.AmbientScope);
        });
    }

    private static void Drain(AmbientEFCoreScopeStack<TestUmbracoDbContext> stack)
    {
        while (stack.AmbientScope is not null)
        {
            stack.Pop();
        }
    }
}
