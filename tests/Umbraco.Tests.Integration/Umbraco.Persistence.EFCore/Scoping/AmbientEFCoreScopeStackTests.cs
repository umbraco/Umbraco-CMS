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

        sut.Push(Mock.Of<IEfCoreScope<TestUmbracoDbContext>>());
        sut.Pop();

        IEfCoreScope<TestUmbracoDbContext> scopeA = Mock.Of<IEfCoreScope<TestUmbracoDbContext>>();
        IEfCoreScope<TestUmbracoDbContext> scopeB = Mock.Of<IEfCoreScope<TestUmbracoDbContext>>();
        (FlowBarriers barriersA, FlowBarriers barriersB) = FlowBarriers.CreatePair();

        async Task<IEfCoreScope<TestUmbracoDbContext>?> RunFlow(
            IEfCoreScope<TestUmbracoDbContext> scope,
            FlowBarriers barriers)
        {
            sut.Push(scope);
            await barriers.EveryFlowHasPushed();

            IEfCoreScope<TestUmbracoDbContext>? ambientScope = sut.AmbientScope;
            await barriers.EveryFlowHasRead();

            sut.Pop();
            return ambientScope;
        }

        IEfCoreScope<TestUmbracoDbContext>?[] ambient = await Task.WhenAll(
            Task.Run(() => RunFlow(scopeA, barriersA)),
            Task.Run(() => RunFlow(scopeB, barriersB)));

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

        IEfCoreScope<TestUmbracoDbContext> outer = Mock.Of<IEfCoreScope<TestUmbracoDbContext>>();
        IEfCoreScope<TestUmbracoDbContext> inner = Mock.Of<IEfCoreScope<TestUmbracoDbContext>>();

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

/// <summary>
/// The two rendezvous points that make an ambient stack isolation test meaningful. Both are load-bearing:
/// every flow pushes before any flow reads, so a stack shared between flows hands the wrong entry to at least
/// one of them; and every flow reads before any flow pops, because a pop that lands first removes the other
/// flow's entry and leaves the remaining read coincidentally correct, hiding the defect.
/// </summary>
internal sealed class FlowBarriers
{
    private readonly TaskCompletionSource _pushed = new();
    private readonly TaskCompletionSource _read = new();
    private FlowBarriers _other = null!;

    public static (FlowBarriers First, FlowBarriers Second) CreatePair()
    {
        var first = new FlowBarriers();
        var second = new FlowBarriers();
        first._other = second;
        second._other = first;

        return (first, second);
    }

    public Task EveryFlowHasPushed()
    {
        _pushed.SetResult();
        return _other._pushed.Task;
    }

    public Task EveryFlowHasRead()
    {
        _read.SetResult();
        return _other._read.Task;
    }
}
