using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;

namespace Umbraco.Tests.Composing
{
    [TestFixture]
    public class ContainerTests
    {
        // tests that a container conforms

        private IContainer GetContainer() => LightInjectContainer.Create();

        [Test]
        public void CanRegisterAndGet()
        {
            var container = GetContainer();

            container.Register<Thing1>();

            var thing = container.GetInstance<Thing1>();
            Assert.IsNotNull(thing);
            Assert.IsInstanceOf<Thing1>(thing);
        }

        [Test]
        public void CanRegisterAndGetLazy()
        {
            var container = GetContainer();

            container.Register<Thing1>();

            var lazyThing = container.GetInstance<Lazy<Thing1>>();
            Assert.IsNotNull(lazyThing);
            Assert.IsInstanceOf<Lazy<Thing1>>(lazyThing);
            var thing = lazyThing.Value;
            Assert.IsNotNull(thing);
            Assert.IsInstanceOf<Thing1>(thing);
        }

        [Test]
        public void CannotRegistedAndGetBase()
        {
            var container = GetContainer();

            container.Register<Thing1>();

            Assert.IsNull(container.TryGetInstance<ThingBase>());
        }

        [Test]
        public void CannotRegisterAndGetInterface()
        {
            var container = GetContainer();

            container.Register<Thing1>();

            Assert.IsNull(container.TryGetInstance<IThing>());
        }

        [Test]
        public void CanRegisterAndGetAllBase()
        {
            var container = GetContainer();

            container.Register<Thing1>();

            var things = container.GetAllInstances<ThingBase>();
            Assert.AreEqual(1, things.Count());

            // lightInject: would be zero with option EnableVariance set to false
        }

        [Test]
        public void CanRegisterAndGetAllInterface()
        {
            var container = GetContainer();

            container.Register<Thing1>();

            var things = container.GetAllInstances<IThing>();
            Assert.AreEqual(1, things.Count());

            // lightInject: would be zero with option EnableVariance set to false
        }

        [Test]
        public void CanRegisterBaseAndGet()
        {
            var container = GetContainer();

            container.Register<ThingBase, Thing1>();

            var thing = container.GetInstance<ThingBase>();
            Assert.IsNotNull(thing);
            Assert.IsInstanceOf<Thing1>(thing);
        }

        [Test]
        public void CanRegisterInterfaceAndGet()
        {
            var container = GetContainer();

            container.Register<IThing, Thing1>();

            var thing = container.GetInstance<IThing>();
            Assert.IsNotNull(thing);
            Assert.IsInstanceOf<Thing1>(thing);
        }

        [Test]
        public void NonSingletonServiceIsNotUnique()
        {
            var container = GetContainer();

            container.Register<IThing, Thing1>();
            container.Register<IThing, Thing2>();

            var things = container.GetInstance<IEnumerable<IThing>>();
            Assert.AreEqual(2, things.Count());

            Assert.IsNull(container.TryGetInstance<IThing>());
        }

        [Test]
        public void SingletonServiceIsUnique()
        {
            var container = GetContainer();

            // for Core services that ppl may want to redefine in components,
            // it is important to be able to have a unique, singleton implementation,
            // and to redefine it - how it's done at container's level depends
            // on each container

            // redefine the service
            container.Register<IThing, Thing1>(Lifetime.Singleton);
            container.Register<IThing, Thing2>(Lifetime.Singleton);

            var things = container.GetInstance<IEnumerable<IThing>>();
            Assert.AreEqual(1, things.Count());

            var thing = container.GetInstance<IThing>();
            Assert.IsInstanceOf<Thing2>(thing);
        }

        [Test]
        public void SingletonImplementationIsNotUnique()
        {
            var container = GetContainer();

            // define two implementations
            container.Register<Thing1>(Lifetime.Singleton);
            container.Register<Thing2>(Lifetime.Singleton);

            var things = container.GetInstance<IEnumerable<IThing>>();
            Assert.AreEqual(2, things.Count());

            Assert.IsNull(container.TryGetInstance<IThing>());
        }

        [Test]
        public void CanInjectEnumerableOfBase()
        {
            var container = GetContainer();

            container.Register<Thing1>();
            container.Register<Thing2>();
            container.Register<NeedThings>();

            var needThings = container.GetInstance<NeedThings>();
            Assert.AreEqual(2, needThings.Things.Count());
        }

        [Test]
        public void CanGetEnumerableOfBase()
        {
            var container = GetContainer();

            container.Register<Thing1>();
            container.Register<Thing2>();

            var things = container.GetInstance<IEnumerable<ThingBase>>();
            Assert.AreEqual(2, things. Count());
        }

        public interface IThing { }

        public abstract class ThingBase : IThing { }
        public class Thing1 : ThingBase { }
        public class Thing2 : ThingBase { }

        public class NeedThings
        {
            public NeedThings(IEnumerable<ThingBase> things)
            {
                Things = things;
            }

            public IEnumerable<ThingBase> Things { get; }
        }
    }
}
