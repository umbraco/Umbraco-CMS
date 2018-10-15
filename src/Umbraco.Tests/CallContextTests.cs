using System.Runtime.Remoting.Messaging;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests
{
    [TestFixture]
    public class CallContextTests
    {
        private static bool _first;

        static CallContextTests()
        {
            SafeCallContext.Register(() =>
            {
                CallContext.FreeNamedDataSlot("test1");
                CallContext.FreeNamedDataSlot("test2");
                return null;
            }, o => {});
        }

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            _first = true;
        }

        // logical call context leaks between tests
        // is is required to clear it before tests begin
        // (don't trust other tests properly tearing down)

        [SetUp]
        public void Setup()
        {
            SafeCallContext.Clear();
        }

        //[TearDown]
        //public void TearDown()
        //{
        //    SafeCallContext.Clear();
        //}

        [Test]
        public void Test1()
        {
            CallContext.LogicalSetData("test1", "test1");
            Assert.IsNull(CallContext.LogicalGetData("test2"));

            CallContext.LogicalSetData("test3b", "test3b");

            if (_first)
            {
                _first = false;
            }
            else
            {
                Assert.IsNotNull(CallContext.LogicalGetData("test3a")); // leak!
            }
        }

        [Test]
        public void Test2()
        {
            CallContext.LogicalSetData("test2", "test2");
            Assert.IsNull(CallContext.LogicalGetData("test1"));
        }

        [Test]
        public void Test3()
        {
            CallContext.LogicalSetData("test3a", "test3a");

            if (_first)
            {
                _first = false;
            }
            else
            {
                Assert.IsNotNull(CallContext.LogicalGetData("test3b")); // leak!
            }
        }
    }
}
