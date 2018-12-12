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

namespace Umbraco.Tests.Components
{
    [TestFixture]
    public class ComponentTests
    {
        private static readonly List<Type> Composed = new List<Type>();
        private static readonly List<string> Initialized = new List<string>();

        private static IFactory MockFactory(Action<Mock<IFactory>> setup = null)
        {
            // fixme use IUmbracoDatabaseFactory vs UmbracoDatabaseFactory, clean it all up!

            var mock = new Mock<IFactory>();

            var logger = Mock.Of<ILogger>();
            var f = new UmbracoDatabaseFactory(logger, new Lazy<IMapperCollection>(() => new MapperCollection(Enumerable.Empty<BaseMapper>())));
            var fs = new FileSystems(mock.Object, logger);
            var p = new ScopeProvider(f, fs, logger);

            mock.Setup(x => x.GetInstance(typeof (ILogger))).Returns(logger);
            mock.Setup(x => x.GetInstance(typeof (IProfilingLogger))).Returns(new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            mock.Setup(x => x.GetInstance(typeof (IUmbracoDatabaseFactory))).Returns(f);
            mock.Setup(x => x.GetInstance(typeof (IScopeProvider))).Returns(p);

            setup?.Invoke(mock);
            return mock.Object;
        }

        private static IRegister MockRegister()
        {
            return Mock.Of<IRegister>();
        }

        private static TypeLoader MockTypeLoader()
        {
            return new TypeLoader();
        }

        public static IRuntimeState MockRuntimeState(RuntimeLevel level)
        {
            var runtimeState = Mock.Of<IRuntimeState>();
            Mock.Get(runtimeState).Setup(x => x.Level).Returns(level);
            return runtimeState;
        }

        [Test]
        public void Boot1A()
        {
            var register = MockRegister();
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Unknown));

            var types = TypeArray<Component1, Component2, Component3, Component4>();
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            // 2 is Core and requires 4
            // 3 is User - goes away with RuntimeLevel.Unknown
            // => reorder components accordingly
            components.Compose();
            AssertTypeArray(TypeArray<Component1, Component4, Component2>(), Composed);
        }

        [Test]
        public void Boot1B()
        {
            var register = MockRegister();
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Run));

            var types = TypeArray<Component1, Component2, Component3, Component4>();
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            // 2 is Core and requires 4
            // 3 is User - stays with RuntimeLevel.Run
            // => reorder components accordingly
            components.Compose();
            AssertTypeArray(TypeArray<Component1, Component4, Component2, Component3>(), Composed);
        }

        [Test]
        public void Boot2()
        {
            var register = MockRegister();
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Unknown));

            var types = TypeArray<Component20, Component21>();
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            // 21 is required by 20
            // => reorder components accordingly
            components.Compose();
            AssertTypeArray(TypeArray<Component21, Component20>(), Composed);
        }

        [Test]
        public void Boot3()
        {
            var register = MockRegister();
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Unknown));

            var types = TypeArray<Component22, Component24, Component25>();
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            // i23 requires 22
            // 24, 25 implement i23
            // 25 required by i23
            // => reorder components accordingly
            components.Compose();
            AssertTypeArray(TypeArray<Component22, Component25, Component24>(), Composed);
        }

        [Test]
        public void BrokenRequire()
        {
            var register = MockRegister();
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Unknown));

            var types = TypeArray<Component1, Component2, Component3>();
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            try
            {
                // 2 is Core and requires 4
                // 4 is missing
                // => throw
                components.Compose();
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
            var register = MockRegister();
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Unknown));

            var types = TypeArray<Component2, Component4, Component13>();
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            // 2 is Core and requires 4
            // 13 is required by 1
            // 1 is missing
            // => reorder components accordingly
            components.Compose();
            AssertTypeArray(TypeArray<Component4, Component2, Component13>(), Composed);
        }

        [Test]
        public void Initialize()
        {
            var register = MockRegister();
            var factory = MockFactory(m =>
            {
                m.Setup(x => x.TryGetInstance(It.Is<Type>(t => t == typeof (ISomeResource)))).Returns(() => new SomeResource());
            });
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Unknown));

            var types = new[] { typeof(Component1), typeof(Component5) };
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            Initialized.Clear();
            components.Compose();
            components.Initialize(factory);
            Assert.AreEqual(2, Composed.Count);
            Assert.AreEqual(typeof(Component1), Composed[0]);
            Assert.AreEqual(typeof(Component5), Composed[1]);
            Assert.AreEqual(1, Initialized.Count);
            Assert.AreEqual("Umbraco.Tests.Components.ComponentTests+SomeResource", Initialized[0]);
        }

        [Test]
        public void Requires1()
        {
            var register = MockRegister();
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Unknown));

            var types = new[] { typeof(Component6), typeof(Component7), typeof(Component8) };
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            components.Compose();
            Assert.AreEqual(2, Composed.Count);
            Assert.AreEqual(typeof(Component6), Composed[0]);
            Assert.AreEqual(typeof(Component8), Composed[1]);
        }

        [Test]
        public void Requires2A()
        {
            var register = MockRegister();
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Unknown));

            var types = new[] { typeof(Component9), typeof(Component2), typeof(Component4) };
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            components.Compose();
            Assert.AreEqual(2, Composed.Count);
            Assert.AreEqual(typeof(Component4), Composed[0]);
            Assert.AreEqual(typeof(Component2), Composed[1]);
            //Assert.AreEqual(typeof(Component9), Composed[2]); -- goes away with RuntimeLevel.Unknown
        }

        [Test]
        public void Requires2B()
        {
            var register = MockRegister();
            var factory = MockFactory();
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Run));

            var types = new[] { typeof(Component9), typeof(Component2), typeof(Component4) };
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            components.Compose();
            components.Initialize(factory);
            Assert.AreEqual(3, Composed.Count);
            Assert.AreEqual(typeof(Component4), Composed[0]);
            Assert.AreEqual(typeof(Component2), Composed[1]);
            Assert.AreEqual(typeof(Component9), Composed[2]);
        }

        [Test]
        public void WeakDependencies()
        {
            var register = MockRegister();
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Unknown));

            var types = new[] { typeof(Component10) };
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            components.Compose();
            Assert.AreEqual(1, Composed.Count);
            Assert.AreEqual(typeof(Component10), Composed[0]);

            types = new[] { typeof(Component11) };
            components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            Assert.Throws<Exception>(() => components.Compose());

            types = new[] { typeof(Component2) };
            components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            Assert.Throws<Exception>(() => components.Compose());

            types = new[] { typeof(Component12) };
            components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            components.Compose();
            Assert.AreEqual(1, Composed.Count);
            Assert.AreEqual(typeof(Component12), Composed[0]);
        }

        [Test]
        public void DisableMissing()
        {
            var register = MockRegister();
            var composition = new Composition(register, MockTypeLoader(), Mock.Of<IProfilingLogger>(), MockRuntimeState(RuntimeLevel.Unknown));

            var types = new[] { typeof(Component6), typeof(Component8) }; // 8 disables 7 which is not in the list
            var components = new Core.Components.Components(composition, types, Mock.Of<IProfilingLogger>());
            Composed.Clear();
            components.Compose();
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
