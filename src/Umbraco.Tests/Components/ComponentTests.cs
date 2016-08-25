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

        public class Component1 : UmbracoComponentBase
        {
            public override void Compose(ServiceContainer container)
            {
                base.Compose(container);
                Composed.Add(GetType());
            }
        }

        [RequireComponent(typeof(Component4))]
        public class Component2 : UmbracoComponentBase, IUmbracoCoreComponent
        {
            public override void Compose(ServiceContainer container)
            {
                base.Compose(container);
                Composed.Add(GetType());
            }
        }

        public class Component3 : UmbracoComponentBase, IUmbracoUserComponent
        {
            public override void Compose(ServiceContainer container)
            {
                base.Compose(container);
                Composed.Add(GetType());
            }
        }

        public class Component4 : UmbracoComponentBase
        {
            public override void Compose(ServiceContainer container)
            {
                base.Compose(container);
                Composed.Add(GetType());
            }
        }

        public class Component5 : UmbracoComponentBase
        {
            public override void Compose(ServiceContainer container)
            {
                base.Compose(container);
                Composed.Add(GetType());
            }

            public void Initialize(ISomeResource resource)
            {
                Initialized.Add(resource.GetType().FullName);
            }
        }

        public interface ISomeResource { }

        public class SomeResource : ISomeResource { }
    }
}
