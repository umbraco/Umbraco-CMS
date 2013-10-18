using System.Linq;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.DynamicsAndReflection
{
    [TestFixture]
    public class ReflectionTests
    {
        [Test]
        public void GetBaseTypesIsOk()
        {
            // tests that the GetBaseTypes extension method works.

            var type = typeof(Class2);
            var types = type.GetBaseTypes(true).ToArray();
            Assert.AreEqual(3, types.Length);
            Assert.Contains(typeof(Class2), types);
            Assert.Contains(typeof(Class1), types);
            Assert.Contains(typeof(object), types);

            types = type.GetBaseTypes(false).ToArray();
            Assert.AreEqual(2, types.Length);
            Assert.Contains(typeof(Class1), types);
            Assert.Contains(typeof(object), types);
        }

        [Test]
        public void GetInterfacesIsOk()
        {
            // tests that GetInterfaces gets _all_ interfaces
            // so the AllInterfaces extension method is useless

            var type = typeof(Class2);
            var interfaces = type.GetInterfaces();
            Assert.AreEqual(2, interfaces.Length);
            Assert.Contains(typeof(IInterface1), interfaces);
            Assert.Contains(typeof(IInterface2), interfaces);
        }

        // TypeExtensions.AllInterfaces was broken an not used, has been commented out
        //
        //[Test]
        //public void AllInterfacesIsBroken()
        //{
        //    // tests that the AllInterfaces extension method is broken
        //
        //    var type = typeof(Class2);
        //    var interfaces = type.AllInterfaces().ToArray();
        //    Assert.AreEqual(3, interfaces.Length); // should be 2!
        //    Assert.Contains(typeof(IInterface1), interfaces);
        //    Assert.Contains(typeof(IInterface2), interfaces);
        //    Assert.AreEqual(2, interfaces.Count(i => i == typeof(IInterface1))); // duplicate!
        //    Assert.AreEqual(1, interfaces.Count(i => i == typeof(IInterface2)));
        //}

        interface IInterface1
        { }

        interface IInterface2 : IInterface1
        {
            void Method();
        }

        class Class1 : IInterface2
        {
            public void Method() { }
        }

        class Class2 : Class1
        { }
    }
}
