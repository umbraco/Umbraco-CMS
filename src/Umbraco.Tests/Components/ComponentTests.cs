using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Components;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Logging;

namespace Umbraco.Tests.Components
{
    [TestFixture]
    public class ComponentTests
    {
        private static readonly List<Type> Composed = new List<Type>();
        private static readonly List<string> Initialized = new List<string>();

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
        }

        [Test]
        public void Boot()
        {
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();

            var logger = Mock.Of<ILogger>();
            var profiler = new LogProfiler(logger);
            container.RegisterInstance(logger);
            container.RegisterInstance(profiler);
            container.RegisterInstance(new ProfilingLogger(logger, profiler));

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new [] { typeof (Component1), typeof (Component2), typeof (Component3), typeof (Component4) });
            Assert.AreEqual(4, Composed.Count);
            Assert.AreEqual(typeof(Component1), Composed[0]);
            Assert.AreEqual(typeof(Component4), Composed[1]);
            Assert.AreEqual(typeof(Component2), Composed[2]);
            Assert.AreEqual(typeof(Component3), Composed[3]);
        }

        [Test]
        public void BrokenDependency()
        {
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();

            var logger = Mock.Of<ILogger>();
            var profiler = new LogProfiler(logger);
            container.RegisterInstance(logger);
            container.RegisterInstance(profiler);
            container.RegisterInstance(new ProfilingLogger(logger, profiler));

            var thing = new BootLoader(container);
            Composed.Clear();
            try
            {
                thing.Boot(new[] { typeof(Component1), typeof(Component2), typeof(Component3) });
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
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();

            container.Register<ISomeResource, SomeResource>();

            var logger = Mock.Of<ILogger>();
            var profiler = new LogProfiler(logger);
            container.RegisterInstance(logger);
            container.RegisterInstance(profiler);
            container.RegisterInstance(new ProfilingLogger(logger, profiler));

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new[] { typeof(Component1), typeof(Component5) });
            Assert.AreEqual(2, Composed.Count);
            Assert.AreEqual(typeof(Component1), Composed[0]);
            Assert.AreEqual(typeof(Component5), Composed[1]);
            Assert.AreEqual(1, Initialized.Count);
            Assert.AreEqual("Umbraco.Tests.Components.ComponentTests+SomeResource", Initialized[0]);
        }

        [Test]
        public void Requires1()
        {
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();

            var logger = Mock.Of<ILogger>();
            var profiler = new LogProfiler(logger);
            container.RegisterInstance(logger);
            container.RegisterInstance(profiler);
            container.RegisterInstance(new ProfilingLogger(logger, profiler));

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new[] { typeof(Component6), typeof(Component7), typeof(Component8) });
            Assert.AreEqual(2, Composed.Count);
            Assert.AreEqual(typeof(Component6), Composed[0]);
            Assert.AreEqual(typeof(Component8), Composed[1]);
        }

        [Test]
        public void Requires2()
        {
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();

            var logger = Mock.Of<ILogger>();
            var profiler = new LogProfiler(logger);
            container.RegisterInstance(logger);
            container.RegisterInstance(profiler);
            container.RegisterInstance(new ProfilingLogger(logger, profiler));

            var thing = new BootLoader(container);
            Composed.Clear();
            thing.Boot(new[] { typeof(Component9), typeof(Component2), typeof(Component4) });
            Assert.AreEqual(3, Composed.Count);
            Assert.AreEqual(typeof(Component4), Composed[0]);
            Assert.AreEqual(typeof(Component2), Composed[1]);
            Assert.AreEqual(typeof(Component9), Composed[2]);
        }

        public class TestComponentBase : UmbracoComponentBase
        {
            public override void Compose(ServiceContainer container)
            {
                base.Compose(container);
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

        public interface ISomeResource { }

        public class SomeResource : ISomeResource { }
    }
}
