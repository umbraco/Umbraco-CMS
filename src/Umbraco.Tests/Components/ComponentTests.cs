using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Components
{
    [TestFixture]
    public class ComponentTests
    {
        private static readonly List<Type> Composed = new List<Type>();
        private static readonly List<string> Initialized = new List<string>();

        private static IContainer MockContainer(Action<Mock<IContainer>> setup = null)
        {
            // fixme use IUmbracoDatabaseFactory vs UmbracoDatabaseFactory, clean it all up!

            var mock = new Mock<IContainer>();

            var testObjects = new TestObjects(null);
            var logger = Mock.Of<ILogger>();
            var s = testObjects.GetDefaultSqlSyntaxProviders(logger);
            var f = new UmbracoDatabaseFactory(s, logger, new MapperCollection(Enumerable.Empty<BaseMapper>()));
            var fs = new FileSystems(mock.Object, logger);
            var p = new ScopeProvider(f, fs, logger);

            mock.Setup(x => x.GetInstance(typeof (ILogger))).Returns(logger);
            mock.Setup(x => x.GetInstance(typeof (ProfilingLogger))).Returns(new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            mock.Setup(x => x.GetInstance(typeof (IUmbracoDatabaseFactory))).Returns(f);
            mock.Setup(x => x.GetInstance(typeof (IScopeProvider))).Returns(p);

            setup?.Invoke(mock);
            return mock.Object;
        }

        [Test]
        public void Boot1A()
        {
            var container = MockContainer();

            var loader = new BootLoader(container);
            Composed.Clear();
            // 2 is Core and requires 4
            // 3 is User - goes away with RuntimeLevel.Unknown
            // => reorder components accordingly
            loader.Boot(TypeArray<Component1, Component2, Component3, Component4>(), RuntimeLevel.Unknown);
            AssertTypeArray(TypeArray<Component1, Component4, Component2>(), Composed);
        }

        [Test]
        public void Boot1B()
        {
            var container = MockContainer();

            var loader = new BootLoader(container);
            Composed.Clear();
            // 2 is Core and requires 4
            // 3 is User - stays with RuntimeLevel.Run
            // => reorder components accordingly
            loader.Boot(TypeArray<Component1, Component2, Component3, Component4>(), RuntimeLevel.Run);
            AssertTypeArray(TypeArray<Component1, Component4, Component2, Component3>(), Composed);
        }

        [Test]
        public void Boot2()
        {
            var container = MockContainer();

            var loader = new BootLoader(container);
            Composed.Clear();
            // 21 is required by 20
            // => reorder components accordingly
            loader.Boot(TypeArray<Component20, Component21>(), RuntimeLevel.Unknown);
            AssertTypeArray(TypeArray<Component21, Component20>(), Composed);
        }

        [Test]
        public void Boot3()
        {
            var container = MockContainer();

            var loader = new BootLoader(container);
            Composed.Clear();
            // i23 requires 22
            // 24, 25 implement i23
            // 25 required by i23
            // => reorder components accordingly
            loader.Boot(TypeArray<Component22, Component24, Component25>(), RuntimeLevel.Unknown);
            AssertTypeArray(TypeArray<Component22, Component25, Component24>(), Composed);
        }

        [Test]
        public void BrokenRequire()
        {
            var container = MockContainer();

            var thing = new BootLoader(container);
            Composed.Clear();
            try
            {
                // 2 is Core and requires 4
                // 4 is missing
                // => throw
                thing.Boot(TypeArray < Component1, Component2, Component3>(), RuntimeLevel.Unknown);
                Assert.Fail("Expected exception.");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Broken component dependency: Umbraco.Tests.Components.ComponentTests+Component2 -> Umbraco.Tests.Components.ComponentTests+Component4.", e.Message);
            }
        }

        [Test]
        public void BrokenRequired()
        {
            var container = MockContainer();

            var thing = new BootLoader(container);
            Composed.Clear();
            // 2 is Core and requires 4
            // 13 is required by 1
            // 1 is missing
            // => reorder components accordingly
            thing.Boot(TypeArray<Component2, Component4, Component13>(), RuntimeLevel.Unknown);
            AssertTypeArray(TypeArray<Component4, Component2, Component13>(), Composed);
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
            Initialized.Clear();
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
        public void Requires2A()
        {
            var container = MockContainer();

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new[] { typeof(Component9), typeof(Component2), typeof(Component4) }, RuntimeLevel.Unknown);
            Assert.AreEqual(2, Composed.Count);
            Assert.AreEqual(typeof(Component4), Composed[0]);
            Assert.AreEqual(typeof(Component2), Composed[1]);
            //Assert.AreEqual(typeof(Component9), Composed[2]); -- goes away with RuntimeLevel.Unknown
        }

        [Test]
        public void Requires2B()
        {
            var container = MockContainer();

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new[] { typeof(Component9), typeof(Component2), typeof(Component4) }, RuntimeLevel.Run);
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

        #region Components

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

        [RequiredByComponent(typeof(Component1))]
        public class Component13 : TestComponentBase
        { }

        public interface ISomeResource { }

        public class SomeResource : ISomeResource { }

        public class Component20 : TestComponentBase
        { }

        [RequiredByComponent(typeof(Component20))]
        public class Component21 : TestComponentBase
        { }

        public class Component22 : TestComponentBase
        { }

        [RequireComponent(typeof(Component22))]
        public interface IComponent23 : IUmbracoComponent
        { }

        public class Component24 : TestComponentBase, IComponent23
        { }

        // should insert itself between 22 and anything i23
        [RequiredByComponent(typeof(IComponent23))]
        //[RequireComponent(typeof(Component22))] - not needed, implement i23
        public class Component25 : TestComponentBase, IComponent23
        { }

        #endregion

        #region TypeArray

        // fixme - move to Testing

        private static Type[] TypeArray<T1>()
        {
            return new[] { typeof(T1) };
        }

        private static Type[] TypeArray<T1, T2>()
        {
            return new[] { typeof(T1), typeof(T2) };
        }

        private static Type[] TypeArray<T1, T2, T3>()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3) };
        }

        private static Type[] TypeArray<T1, T2, T3, T4>()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
        }

        private static Type[] TypeArray<T1, T2, T3, T4, T5>()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
        }

        private static Type[] TypeArray<T1, T2, T3, T4, T5, T6>()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
        }

        private static Type[] TypeArray<T1, T2, T3, T4, T5, T6, T7>()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
        }

        private static Type[] TypeArray<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            return new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
        }

        private static void AssertTypeArray(IReadOnlyList<Type> expected, IReadOnlyList<Type> test)
        {
            Assert.AreEqual(expected.Count, test.Count);
            for (var i = 0; i < expected.Count; i++)
                Assert.AreEqual(expected[i], test[i]);
        }

        #endregion
    }
}
