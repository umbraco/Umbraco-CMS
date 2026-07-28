using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Scoping
{
    [TestFixture]
    public class AmbientScopeStackTests
    {
        [Test]
        public async Task Push_IsolatesConcurrentFlowsThatInheritedAnEmptyStack()
        {
            var sut = new AmbientScopeStack();

            while (sut.AmbientScope is not null)
            {
                sut.Pop();
            }

            // Any scope used before hosted services start leaves a non-null but empty stack on the execution
            // context. Every flow branching off that context inherits the same instance, because AsyncLocal
            // copy-on-write protects the reference, not the object it points at.
            sut.Push(Mock.Of<IScope>());
            sut.Pop();

            IScope scopeA = Mock.Of<IScope>();
            IScope scopeB = Mock.Of<IScope>();
            (FlowBarriers barriersA, FlowBarriers barriersB) = FlowBarriers.CreatePair();

            async Task<IScope?> RunFlow(IScope scope, FlowBarriers barriers)
            {
                sut.Push(scope);
                await barriers.EveryFlowHasPushed();

                IScope? ambientScope = sut.AmbientScope;
                await barriers.EveryFlowHasRead();

                sut.Pop();
                return ambientScope;
            }

            IScope?[] ambient = await Task.WhenAll(
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
            var sut = new AmbientScopeStack();

            while (sut.AmbientScope is not null)
            {
                sut.Pop();
            }

            IScope outer = Mock.Of<IScope>();
            IScope inner = Mock.Of<IScope>();

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
    }

    /// <summary>
    /// The scope context stack shares the defect and the fix of <see cref="AmbientScopeStack" />. It is covered
    /// separately because isolating only the scope stack moves the corruption here: scopes that stop being wrongly
    /// treated as nested start pushing a context of their own, onto a stack that is still shared between flows.
    /// </summary>
    [TestFixture]
    public class AmbientScopeContextStackTests
    {
        [Test]
        public async Task Push_IsolatesConcurrentFlowsThatInheritedAnEmptyStack()
        {
            var sut = new AmbientScopeContextStack();

            while (sut.AmbientContext is not null)
            {
                sut.Pop();
            }

            sut.Push(Mock.Of<IScopeContext>());
            sut.Pop();

            IScopeContext contextA = Mock.Of<IScopeContext>();
            IScopeContext contextB = Mock.Of<IScopeContext>();
            (FlowBarriers barriersA, FlowBarriers barriersB) = FlowBarriers.CreatePair();

            async Task<IScopeContext?> RunFlow(IScopeContext context, FlowBarriers barriers)
            {
                sut.Push(context);
                await barriers.EveryFlowHasPushed();

                IScopeContext? ambientContext = sut.AmbientContext;
                await barriers.EveryFlowHasRead();

                sut.Pop();
                return ambientContext;
            }

            IScopeContext?[] ambient = await Task.WhenAll(
                Task.Run(() => RunFlow(contextA, barriersA)),
                Task.Run(() => RunFlow(contextB, barriersB)));

            Assert.Multiple(() =>
            {
                Assert.AreSame(contextA, ambient[0], "Flow A observed another flow's ambient context.");
                Assert.AreSame(contextB, ambient[1], "Flow B observed another flow's ambient context.");
                Assert.IsNull(sut.AmbientContext, "A context pushed in a branching flow leaked into the calling context.");
            });
        }

        [Test]
        public void Push_KeepsNestedContextsOnTheSameStack()
        {
            var sut = new AmbientScopeContextStack();

            while (sut.AmbientContext is not null)
            {
                sut.Pop();
            }

            IScopeContext outer = Mock.Of<IScopeContext>();
            IScopeContext inner = Mock.Of<IScopeContext>();

            sut.Push(outer);
            sut.Push(inner);

            Assert.Multiple(() =>
            {
                Assert.AreSame(inner, sut.AmbientContext);
                Assert.AreSame(inner, sut.Pop());
                Assert.AreSame(outer, sut.AmbientContext);
                Assert.AreSame(outer, sut.Pop());
                Assert.IsNull(sut.AmbientContext);
            });
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
}
