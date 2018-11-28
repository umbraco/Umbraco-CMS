using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Composing;

namespace Umbraco.Tests.Composing
{
    // TODO
    // this class should contain everything to ensure that a container implementation
    // complies with Umbraco's requirements.

    [TestFixture]
    public class ContainerImplementationTests // FIXME merge into ContainerTests or ContainerConformingTests
    {
        private IRegister CreateRegister() => RegisterFactory.Create();

        [Test]
        public void CanRegisterSingletonInterface()
        {
            var register = CreateRegister();
            register.RegisterSingleton<ITestInterface, TestClass1>();
            var factory = register.CreateFactory();
            var s1 = factory.GetInstance<ITestInterface>();
            var s2 = factory.GetInstance<ITestInterface>();
            Assert.AreSame(s1, s2);
        }

        [Test]
        public void CanRegisterSingletonClass()
        {
            var register = CreateRegister();
            register.RegisterSingleton<TestClass1>();
            var factory = register.CreateFactory();
            var s1 = factory.GetInstance<TestClass1>();
            var s2 = factory.GetInstance<TestClass1>();
            Assert.AreSame(s1, s2);
        }

        [Test]
        public void CanReRegisterSingletonInterface()
        {
            var register = CreateRegister();
            register.RegisterSingleton<ITestInterface, TestClass1>();
            register.RegisterSingleton<ITestInterface, TestClass2>();
            var factory = register.CreateFactory();
            var s = factory.GetInstance<ITestInterface>();
            Assert.IsInstanceOf<TestClass2>(s);
        }

        [Test]
        public void CanRegisterSingletonWithCreate()
        {
            var register = CreateRegister();
            register.RegisterSingleton(c => c.CreateInstance<TestClass3>(new TestClass1()));
            var factory = register.CreateFactory();
            var s1 = factory.GetInstance<TestClass3>();
            var s2 = factory.GetInstance<TestClass3>();
            Assert.AreSame(s1, s2);
        }

        public interface ITestInterface{}

        public class TestClass1 : ITestInterface{}

        public class TestClass2 : ITestInterface{}

        public class TestClass3
        {
            public TestClass3(TestClass1 c) {}
        }
    }
}
