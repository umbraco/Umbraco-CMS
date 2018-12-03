using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.CoreThings
{
    [TestFixture]
    public class AttemptTests
    {

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
