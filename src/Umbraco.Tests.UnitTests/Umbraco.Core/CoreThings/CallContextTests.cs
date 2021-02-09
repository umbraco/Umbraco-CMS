// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Core;
using Umbraco.Core.Scoping;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.CoreThings
{
    [TestFixture]
    public class CallContextTests
    {
        private static bool s_first;

        static CallContextTests() => SafeCallContext.Register(
            () =>
                {
                    CallContext<string>.SetData("test1", null);
                    CallContext<string>.SetData("test2", null);
                    return null;
                }, o => { });

        [OneTimeSetUp]
        public void SetUpFixture() => s_first = true;

        // logical call context leaks between tests
        // is is required to clear it before tests begin
        // (don't trust other tests properly tearing down)
        [SetUp]
        public void Setup() => SafeCallContext.Clear();

        [TearDown]
        public void TearDown() => SafeCallContext.Clear();

        [Test]
        public void Test1()
        {
            CallContext<string>.SetData("test1", "test1");
            Assert.IsNull(CallContext<string>.GetData("test2"));

            CallContext<string>.SetData("test3b", "test3b");

            if (s_first)
            {
                s_first = false;
            }
            else
            {
                Assert.IsNotNull(CallContext<string>.GetData("test3a")); // leak!
            }
        }

        [Test]
        public void Test2()
        {
            CallContext<string>.SetData("test2", "test2");
            Assert.IsNull(CallContext<string>.GetData("test1"));
        }

        [Test]
        public void Test3()
        {
            CallContext<string>.SetData("test3a", "test3a");

            if (s_first)
            {
                s_first = false;
            }
            else
            {
                Assert.IsNotNull(CallContext<string>.GetData("test3b")); // leak!
            }
        }
    }
}
