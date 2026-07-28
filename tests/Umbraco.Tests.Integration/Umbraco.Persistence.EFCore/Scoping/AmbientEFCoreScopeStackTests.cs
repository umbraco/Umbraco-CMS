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
        var readA = new TaskCompletionSource();
        var readB = new TaskCompletionSource();

        // Two barriers, both load-bearing. Every flow pushes before any flow reads, so a shared stack yields the
        // wrong entry to at least one of them. Every flow also reads before any flow pops, otherwise a pop could
        // remove the other flow's entry first and leave the remaining read coincidentally correct.
        async Task<IEFCoreScope<TestUmbracoDbContext>?> RunFlow(
            IEFCoreScope<TestUmbracoDbContext> scope,
            TaskCompletionSource pushed,
            Task otherPushed,
            TaskCompletionSource read,
            Task otherRead)
        {
            sut.Push(scope);
            pushed.SetResult();
            await otherPushed;

            IEFCoreScope<TestUmbracoDbContext>? ambientScope = sut.AmbientScope;
            read.SetResult();
            await otherRead;

            sut.Pop();
            return ambientScope;
        }

        IEFCoreScope<TestUmbracoDbContext>?[] ambient = await Task.WhenAll(
            Task.Run(() => RunFlow(scopeA, pushedA, pushedB.Task, readA, readB.Task)),
            Task.Run(() => RunFlow(scopeB, pushedB, pushedA.Task, readB, readA.Task)));

        Assert.Multiple(() =>
        {
            Assert.AreSame(scopeA, ambient[0], "Flow A observed another flow's ambient scope.");
            Assert.AreSame(scopeB, ambient[1], "Flow B observed another flow's ambient scope.");
            Assert.IsNull(sut.AmbientScope, "A scope pushed in a branching flow leaked into the calling context.");
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
