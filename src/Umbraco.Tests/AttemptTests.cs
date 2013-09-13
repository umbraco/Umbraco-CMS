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
            Attempt.Try(Attempt.Succeed("success!"),
                s => Assert.AreEqual("success!", s),
                exception => Assert.Fail("Should have been successful."))

                // previous one was a success so that one SHOULD NOT run
                // and report success
                .OnFailure(() => Attempt.Succeed(123),
                    i => Assert.Fail("The previous attempt was successful!"),
                    exception => Assert.Fail("The previous attempt was successful!"))

                // previous one did not run, last run was a success so that one SHOULD run
                // and report failure
                .OnSuccess(() => Attempt<double>.Fail(new Exception("Failed!")),
                    d => Assert.Fail("Should have failed."),
                    exception => Assert.AreEqual("Failed!", exception.Message))

                // previous one did run and was a failure so that one SHOULD NOT run
                .OnSuccess(() => Attempt.Succeed(987),
                    i => Assert.Fail("The previous attempt failed!"),
                    exception => Assert.Fail("The previous attempt failed!"))

                // previous one did not run, last run was a failure so that one SHOULD run
                // then why does it run?
                .OnFailure(() => Attempt.Succeed("finished"),
                    i => Assert.AreEqual("finished", i),
                    exception => Assert.Fail("Should have been successful."));
        }
    }
}
