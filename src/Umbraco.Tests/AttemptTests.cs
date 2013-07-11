using System;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests
{
    [TestFixture]
    public class AttemptTests
    {

        [Test]
        public void Chained_Attempts()
        {
            Attempt<string>.Try(new Attempt<string>(true, "success!"),
                                s => Assert.AreEqual("success!", s),
                                exception => Assert.Fail("It was successful"))
                           .IfFailed(() => new Attempt<int>(true, 123),
                                     i => Assert.Fail("The previous attempt was successful!"),
                                     exception => Assert.Fail("The previous attempt was successful!"))
                           .IfSuccessful(() => new Attempt<double>(new Exception("Failed!")),
                                         d => Assert.Fail("An exception was thrown"),
                                         exception => Assert.AreEqual("Failed!", exception.Message))
                           .IfSuccessful(() => new Attempt<int>(true, 987),
                                         i => Assert.Fail("The previous attempt failed!"),
                                         exception => Assert.Fail("The previous attempt failed!"))
                           .IfFailed(() => new Attempt<string>(true, "finished"),
                                     i => Assert.AreEqual("finished", i),
                                     exception => Assert.Fail("It was successful"));
        }

    }
}