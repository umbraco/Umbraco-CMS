using System.Linq;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Clr
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

        #region Test Objects

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

        #endregion
    }
}
