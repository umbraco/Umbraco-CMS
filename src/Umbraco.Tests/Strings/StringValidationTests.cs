using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Strings
{
    [TestFixture]
    public class StringValidationTests : UmbracoTestBase
    {
        [Test]
        public void Validate_Email_Address()
        {
            var foo = new EmailAddressAttribute();

            Assert.IsTrue(foo.IsValid("someone@somewhere.com"));
            Assert.IsTrue(foo.IsValid("someone@somewhere.co.uk"));
            Assert.IsTrue(foo.IsValid("someone+tag@somewhere.net"));
            Assert.IsTrue(foo.IsValid("futureTLD@somewhere.fooo"));

            Assert.IsTrue(foo.IsValid("abc@xyz.financial"));

            Assert.IsFalse(foo.IsValid("fdsa"));
            Assert.IsFalse(foo.IsValid("fdsa@"));
            Assert.IsFalse(foo.IsValid("fdsa@fdsa"));
            Assert.IsFalse(foo.IsValid("fdsa@fdsa."));

        }
    }
}