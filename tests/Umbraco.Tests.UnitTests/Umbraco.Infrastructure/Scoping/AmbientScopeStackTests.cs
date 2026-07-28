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
            var pushedA = new TaskCompletionSource();
            var pushedB = new TaskCompletionSource();

            async Task<IScope?> PushThenReadAmbient(IScope scope, TaskCompletionSource pushed, Task otherPushed)
            {
                sut.Push(scope);
                pushed.SetResult();
                await otherPushed;
                return sut.AmbientScope;
            }

            IScope?[] ambient = await Task.WhenAll(
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
            var pushedA = new TaskCompletionSource();
            var pushedB = new TaskCompletionSource();

            async Task<IScopeContext?> PushThenReadAmbient(
                IScopeContext context,
                TaskCompletionSource pushed,
                Task otherPushed)
            {
                sut.Push(context);
                pushed.SetResult();
                await otherPushed;
                return sut.AmbientContext;
            }

            IScopeContext?[] ambient = await Task.WhenAll(
                Task.Run(() => PushThenReadAmbient(contextA, pushedA, pushedB.Task)),
                Task.Run(() => PushThenReadAmbient(contextB, pushedB, pushedA.Task)));

            Assert.Multiple(() =>
            {
                Assert.AreSame(contextA, ambient[0], "Flow A observed another flow's ambient context.");
                Assert.AreSame(contextB, ambient[1], "Flow B observed another flow's ambient context.");
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
}
