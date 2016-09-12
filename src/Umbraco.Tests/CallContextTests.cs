using System.Runtime.Remoting.Messaging;
using NUnit.Framework;

namespace Umbraco.Tests
{
    [TestFixture]
    public class CallContextTests
    {
        [SetUp]
        public void Setup()
        {
            ClearCallContext();
        }

        [TearDown]
        public void TearDown()
        {
            ClearCallContext();
        }

        private static void ClearCallContext()
        {
            // logical call context leaks between tests
            // cleanup things before/after we've run else one of
            // the two tests will fail
            CallContext.FreeNamedDataSlot("test1");
            CallContext.FreeNamedDataSlot("test2");
        }

        [Test]
        public void Test1()
        {
            CallContext.LogicalSetData("test1", "test1");
            Assert.IsNull(CallContext.LogicalGetData("test2"));
        }

        [Test]
        public void Test2()
        {
            CallContext.LogicalSetData("test2", "test2");
            Assert.IsNull(CallContext.LogicalGetData("test1"));
        }
    }
}
