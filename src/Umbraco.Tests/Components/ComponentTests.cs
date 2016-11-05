using System;
using System.Collections.Generic;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Logging;

namespace Umbraco.Tests.Components
{
    [TestFixture]
    public class ComponentTests
    {
        private static readonly List<Type> Composed = new List<Type>();
        private static readonly List<string> Initialized = new List<string>();

        private static IServiceContainer MockContainer(Action<Mock<IServiceContainer>> setup = null)
        {
            var mock = new Mock<IServiceContainer>();
            mock.Setup(x => x.GetInstance<ProfilingLogger>()).Returns(new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            setup?.Invoke(mock);
            return mock.Object;
        }

        [Test]
        public void Boot()
        {
            var container = MockContainer();

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new [] { typeof (Component1), typeof (Component2), typeof (Component3), typeof (Component4) }, RuntimeLevel.Unknown);
            Assert.AreEqual(4, Composed.Count);
            Assert.AreEqual(typeof(Component1), Composed[0]);
            Assert.AreEqual(typeof(Component4), Composed[1]);
            Assert.AreEqual(typeof(Component2), Composed[2]);
            Assert.AreEqual(typeof(Component3), Composed[3]);
        }

        [Test]
        public void BrokenDependency()
        {
            var container = MockContainer();

            var thing = new BootLoader(container);
            Composed.Clear();
            try
            {
                thing.Boot(new[] { typeof(Component1), typeof(Component2), typeof(Component3) }, RuntimeLevel.Unknown);
                Assert.Fail("Expected exception.");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Broken component dependency: Umbraco.Tests.Components.ComponentTests+Component2 -> Umbraco.Tests.Components.ComponentTests+Component4.", e.Message);
            }
        }

        [Test]
        public void Initialize()
        {
            var container = MockContainer(m =>
            {
                m.Setup(x => x.TryGetInstance(It.Is<Type>(t => t == typeof (ISomeResource)))).Returns(() => new SomeResource());
            });

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new[] { typeof(Component1), typeof(Component5) }, RuntimeLevel.Unknown);
            Assert.AreEqual(2, Composed.Count);
            Assert.AreEqual(typeof(Component1), Composed[0]);
            Assert.AreEqual(typeof(Component5), Composed[1]);
            Assert.AreEqual(1, Initialized.Count);
            Assert.AreEqual("Umbraco.Tests.Components.ComponentTests+SomeResource", Initialized[0]);
        }

        [Test]
        public void Requires1()
        {
            var container = MockContainer();

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new[] { typeof(Component6), typeof(Component7), typeof(Component8) }, RuntimeLevel.Unknown);
            Assert.AreEqual(2, Composed.Count);
            Assert.AreEqual(typeof(Component6), Composed[0]);
            Assert.AreEqual(typeof(Component8), Composed[1]);
        }

        [Test]
        public void Requires2()
        {
            var container = MockContainer();

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new[] { typeof(Component9), typeof(Component2), typeof(Component4) }, RuntimeLevel.Unknown);
            Assert.AreEqual(3, Composed.Count);
            Assert.AreEqual(typeof(Component4), Composed[0]);
            Assert.AreEqual(typeof(Component2), Composed[1]);
            Assert.AreEqual(typeof(Component9), Composed[2]);
        }

        [Test]
        public void WeakDependencies()
        {
            var container = MockContainer();

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new[] { typeof(Component10) }, RuntimeLevel.Unknown);
            Assert.AreEqual(1, Composed.Count);
            Assert.AreEqual(typeof(Component10), Composed[0]);

            thing = new BootLoader(container);
            Composed.Clear();
            Assert.Throws<Exception>(() => thing.Boot(new[] { typeof(Component11) }, RuntimeLevel.Unknown));

            thing = new BootLoader(container);
            Composed.Clear();
            Assert.Throws<Exception>(() => thing.Boot(new[] { typeof(Component2) }, RuntimeLevel.Unknown));

            thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new[] { typeof(Component12) }, RuntimeLevel.Unknown);
            Assert.AreEqual(1, Composed.Count);
            Assert.AreEqual(typeof(Component12), Composed[0]);
        }

        [Test]
        public void DisableMissing()
        {
            var container = MockContainer();

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new[] { typeof(Component6), typeof(Component8) }, RuntimeLevel.Unknown); // 8 disables 7 which is not in the list
            Assert.AreEqual(2, Composed.Count);
            Assert.AreEqual(typeof(Component6), Composed[0]);
            Assert.AreEqual(typeof(Component8), Composed[1]);
        }

        public class TestComponentBase : UmbracoComponentBase
        {
            public override void Compose(Composition composition)
            {
                base.Compose(composition);
                Composed.Add(GetType());
            }
        }

        public class Component1 : TestComponentBase
        { }

        [RequireComponent(typeof(Component4))]
        public class Component2 : TestComponentBase, IUmbracoCoreComponent
        { }

        public class Component3 : TestComponentBase, IUmbracoUserComponent
        { }

        public class Component4 : TestComponentBase
        { }

        public class Component5 : TestComponentBase
        {
            public void Initialize(ISomeResource resource)
            {
                Initialized.Add(resource.GetType().FullName);
            }
        }

        [DisableComponent]
        public class Component6 : TestComponentBase
        { }

        public class Component7 : TestComponentBase
        { }

        [DisableComponent(typeof(Component7))]
        [EnableComponent(typeof(Component6))]
        public class Component8 : TestComponentBase
        { }

        public interface ITestComponent : IUmbracoUserComponent
        { }

        public class Component9 : TestComponentBase, ITestComponent
        { }

        [RequireComponent(typeof(ITestComponent))]
        public class Component10 : TestComponentBase
        { }

        [RequireComponent(typeof(ITestComponent), false)]
        public class Component11 : TestComponentBase
        { }

        [RequireComponent(typeof(Component4), true)]
        public class Component12 : TestComponentBase, IUmbracoCoreComponent
        { }

        public interface ISomeResource { }

        public class SomeResource : ISomeResource { }
    }
}
