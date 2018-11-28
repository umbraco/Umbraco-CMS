using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;

namespace Umbraco.Tests.Composing
{
    [TestFixture]
    public class ContainerConfirmingTests
    {
        // tests that a container conforms

        private IRegister GetRegister() => LightInjectContainer.Create();

        [Test]
        public void CanRegisterAndGet()
        {
            var register = GetRegister();

            register.Register<Thing1>();

            var factory = register.CreateFactory();

            var thing = factory.GetInstance<Thing1>();
            Assert.IsNotNull(thing);
            Assert.IsInstanceOf<Thing1>(thing);
        }

        [Test]
        public void CanRegisterAndGetLazy()
        {
            var register = GetRegister();

            register.Register<Thing1>();

            var factory = register.CreateFactory();

            var lazyThing = factory.GetInstance<Lazy<Thing1>>();
            Assert.IsNotNull(lazyThing);
            Assert.IsInstanceOf<Lazy<Thing1>>(lazyThing);
            var thing = lazyThing.Value;
            Assert.IsNotNull(thing);
            Assert.IsInstanceOf<Thing1>(thing);
        }

        [Test]
        public void CannotRegistedAndGetBase()
        {
            var register = GetRegister();

            register.Register<Thing1>();

            var factory = register.CreateFactory();

            Assert.IsNull(factory.TryGetInstance<ThingBase>());
        }

        [Test]
        public void CannotRegisterAndGetInterface()
        {
            var register = GetRegister();

            register.Register<Thing1>();

            var factory = register.CreateFactory();

            Assert.IsNull(factory.TryGetInstance<IThing>());
        }

        [Test]
        public void CanRegisterAndGetAllBase()
        {
            var register = GetRegister();

            register.Register<Thing1>();

            var factory = register.CreateFactory();

            var things = factory.GetAllInstances<ThingBase>();
            Assert.AreEqual(1, things.Count());

            // lightInject: would be zero with option EnableVariance set to false
        }

        [Test]
        public void CanRegisterAndGetAllInterface()
        {
            var register = GetRegister();

            register.Register<Thing1>();

            var factory = register.CreateFactory();

            var things = factory.GetAllInstances<IThing>();
            Assert.AreEqual(1, things.Count());

            // lightInject: would be zero with option EnableVariance set to false
        }

        [Test]
        public void CanRegisterBaseAndGet()
        {
            var register = GetRegister();

            register.Register<ThingBase, Thing1>();

            var factory = register.CreateFactory();

            var thing = factory.GetInstance<ThingBase>();
            Assert.IsNotNull(thing);
            Assert.IsInstanceOf<Thing1>(thing);
        }

        [Test]
        public void CanRegisterInterfaceAndGet()
        {
            var register = GetRegister();

            register.Register<IThing, Thing1>();

            var factory = register.CreateFactory();

            var thing = factory.GetInstance<IThing>();
            Assert.IsNotNull(thing);
            Assert.IsInstanceOf<Thing1>(thing);
        }

        [Test]
        public void NonSingletonServiceIsNotUnique()
        {
            var register = GetRegister();

            register.Register<IThing, Thing1>();
            register.Register<IThing, Thing2>();

            var factory = register.CreateFactory();

            var things = factory.GetInstance<IEnumerable<IThing>>();
            Assert.AreEqual(2, things.Count());

            Assert.IsNull(factory.TryGetInstance<IThing>());
        }

        [Test]
        public void SingletonServiceIsUnique()
        {
            var register = GetRegister();

            // for Core services that ppl may want to redefine in components,
            // it is important to be able to have a unique, singleton implementation,
            // and to redefine it - how it's done at container's level depends
            // on each container

            // redefine the service
            register.Register<IThing, Thing1>(Lifetime.Singleton);
            register.Register<IThing, Thing2>(Lifetime.Singleton);

            var factory = register.CreateFactory();

            var things = factory.GetInstance<IEnumerable<IThing>>();
            Assert.AreEqual(1, things.Count());

            var thing = factory.GetInstance<IThing>();
            Assert.IsInstanceOf<Thing2>(thing);
        }

        [Test]
        public void SingletonImplementationIsNotUnique()
        {
            var register = GetRegister();

            // define two implementations
            register.Register<Thing1>(Lifetime.Singleton);
            register.Register<Thing2>(Lifetime.Singleton);

            var factory = register.CreateFactory();

            var things = factory.GetInstance<IEnumerable<IThing>>();
            Assert.AreEqual(2, things.Count());

            Assert.IsNull(factory.TryGetInstance<IThing>());
        }

        [Test]
        public void CanInjectEnumerableOfBase()
        {
            var register = GetRegister();

            register.Register<Thing1>();
            register.Register<Thing2>();
            register.Register<NeedThings>();

            var factory = register.CreateFactory();

            var needThings = factory.GetInstance<NeedThings>();
            Assert.AreEqual(2, needThings.Things.Count());
        }

        [Test]
        public void CanGetEnumerableOfBase()
        {
            var register = GetRegister();

            register.Register<Thing1>();
            register.Register<Thing2>();

            var factory = register.CreateFactory();

            var things = factory.GetInstance<IEnumerable<ThingBase>>();
            Assert.AreEqual(2, things. Count());
        }

        [Test]
        public void CanRegisterSingletonInterface()
        {
            var register = GetRegister();
            register.RegisterSingleton<IThing, Thing1>();
            var factory = register.CreateFactory();
            var s1 = factory.GetInstance<IThing>();
            var s2 = factory.GetInstance<IThing>();
            Assert.AreSame(s1, s2);
        }

        [Test]
        public void CanRegisterSingletonClass()
        {
            var register = GetRegister();
            register.RegisterSingleton<Thing1>();
            var factory = register.CreateFactory();
            var s1 = factory.GetInstance<Thing1>();
            var s2 = factory.GetInstance<Thing1>();
            Assert.AreSame(s1, s2);
        }

        [Test]
        public void CanReRegisterSingletonInterface()
        {
            var register = GetRegister();
            register.RegisterSingleton<IThing, Thing1>();
            register.RegisterSingleton<IThing, Thing2>();
            var factory = register.CreateFactory();
            var s = factory.GetInstance<IThing>();
            Assert.IsInstanceOf<Thing2>(s);
        }

        [Test]
        public void CanRegisterSingletonWithCreate()
        {
            var register = GetRegister();
            register.RegisterSingleton(c => c.CreateInstance<Thing3>(new Thing1()));
            var factory = register.CreateFactory();
            var s1 = factory.GetInstance<Thing3>();
            var s2 = factory.GetInstance<Thing3>();
            Assert.AreSame(s1, s2);
        }

        public interface IThing { }

        public abstract class ThingBase : IThing { }
        public class Thing1 : ThingBase { }
        public class Thing2 : ThingBase { }

        public class Thing3 : ThingBase
        {
            public Thing3(Thing1 thing) { }
        }

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
