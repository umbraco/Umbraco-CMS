using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Scoping
{
    /// <summary>
    /// Both ambient stacks keep their state in a static <see cref="AsyncLocal{T}" /> and share the same defect and
    /// the same fix, so they share these tests. They expose no common interface, so each concrete stack is reached
    /// through the abstract members below.
    /// </summary>
    /// <typeparam name="TItem">The type the stack holds.</typeparam>
    public abstract class AmbientStackTestsBase<TItem>
        where TItem : class
    {
        [Test]
        public async Task Push_IsolatesConcurrentFlowsThatInheritedAnEmptyStack()
        {
            Drain();

            // Any scope used before hosted services start leaves a non-null but empty stack on the execution
            // context. Every flow branching off that context inherits the same instance, because AsyncLocal
            // copy-on-write protects the reference, not the object it points at.
            Push(CreateItem());
            Pop();

            TItem itemA = CreateItem();
            TItem itemB = CreateItem();
            (FlowBarriers barriersA, FlowBarriers barriersB) = FlowBarriers.CreatePair();

            async Task<TItem?> RunFlow(TItem item, FlowBarriers barriers)
            {
                Push(item);
                await barriers.EveryFlowHasPushed();

                TItem? ambient = Peek();
                await barriers.EveryFlowHasRead();

                Pop();
                return ambient;
            }

            TItem?[] ambient = await Task.WhenAll(
                Task.Run(() => RunFlow(itemA, barriersA)),
                Task.Run(() => RunFlow(itemB, barriersB)));

            Assert.Multiple(() =>
            {
                Assert.AreSame(itemA, ambient[0], "Flow A observed another flow's ambient entry.");
                Assert.AreSame(itemB, ambient[1], "Flow B observed another flow's ambient entry.");
                Assert.IsNull(Peek(), "An entry pushed in a branching flow leaked into the calling context.");
            });
        }

        [Test]
        public void Push_KeepsNestedEntriesOnTheSameStack()
        {
            Drain();

            TItem outer = CreateItem();
            TItem inner = CreateItem();

            Push(outer);
            Push(inner);

            Assert.Multiple(() =>
            {
                Assert.AreSame(inner, Peek());
                Assert.AreSame(inner, Pop());
                Assert.AreSame(outer, Peek());
                Assert.AreSame(outer, Pop());
                Assert.IsNull(Peek());
            });
        }

        protected abstract TItem? Peek();

        protected abstract void Push(TItem item);

        protected abstract TItem Pop();

        protected abstract TItem CreateItem();

        /// <summary>
        /// The stack is held in a static <see cref="AsyncLocal{T}" />, so a synchronous test can observe entries
        /// left behind by an earlier one.
        /// </summary>
        private void Drain()
        {
            while (Peek() is not null)
            {
                Pop();
            }
        }
    }

    [TestFixture]
    public class AmbientScopeStackTests : AmbientStackTestsBase<IScope>
    {
        private readonly AmbientScopeStack _sut = new();

        protected override IScope? Peek() => _sut.AmbientScope;

        protected override void Push(IScope item) => _sut.Push(item);

        protected override IScope Pop() => _sut.Pop();

        protected override IScope CreateItem() => Mock.Of<IScope>();
    }

    [TestFixture]
    public class AmbientScopeContextStackTests : AmbientStackTestsBase<IScopeContext>
    {
        private readonly AmbientScopeContextStack _sut = new();

        protected override IScopeContext? Peek() => _sut.AmbientContext;

        protected override void Push(IScopeContext item) => _sut.Push(item);

        protected override IScopeContext Pop() => _sut.Pop();

        protected override IScopeContext CreateItem() => Mock.Of<IScopeContext>();
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
