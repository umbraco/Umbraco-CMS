using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Composing;

namespace Umbraco.Tests.Composing
{
    // TODO
    // this class should contain everything to ensure that a container implementation
    // complies with Umbraco's requirements.

    [TestFixture]
    public class ContainerImplementationTests
    {
        private IContainer CreateContainer() => ContainerFactory.Create();

        [Test]
        public void CanRegisterSingletonInterface()
        {
            var container = CreateContainer();
            container.RegisterSingleton<ITestInterface, TestClass1>();
            var s1 = container.GetInstance<ITestInterface>();
            var s2 = container.GetInstance<ITestInterface>();
            Assert.AreSame(s1, s2);
        }

        [Test]
        public void CanRegisterSingletonClass()
        {
            var container = CreateContainer();
            container.RegisterSingleton<TestClass1>();
            var s1 = container.GetInstance<TestClass1>();
            var s2 = container.GetInstance<TestClass1>();
            Assert.AreSame(s1, s2);
        }

        [Test]
        public void CanReRegisterSingletonInterface()
        {
            var container = CreateContainer();
            container.RegisterSingleton<ITestInterface, TestClass1>();
            container.RegisterSingleton<ITestInterface, TestClass2>();
            var s = container.GetInstance<ITestInterface>();
            Assert.IsInstanceOf<TestClass2>(s);
        }

        [Test]
        public void CanRegisterSingletonWithCreate()
        {
            var container = CreateContainer();
            container.RegisterSingleton(c => c.CreateInstance<TestClass3>(new TestClass1()));
            var s1 = container.GetInstance<TestClass3>();
            var s2 = container.GetInstance<TestClass3>();
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
