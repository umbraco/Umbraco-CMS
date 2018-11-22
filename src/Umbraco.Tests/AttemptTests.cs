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

        [Test]
        public void AttemptIf()
        {
            // just making sure that it is ok to use TryParse as a condition

            int value;
            var attempt = Attempt.If(int.TryParse("1234", out value), value);
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(1234, attempt.Result);

            attempt = Attempt.If(int.TryParse("12xxx34", out value), value);
            Assert.IsFalse(attempt.Success);
        }
    }
}
