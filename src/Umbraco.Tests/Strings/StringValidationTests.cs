using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

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
            Assert.IsTrue(foo.IsValid("admin+gmail-syntax@c.pizza"));
            Assert.IsTrue(foo.IsValid("admin@c.pizza"));

            Assert.IsFalse(foo.IsValid("fdsa"));
            Assert.IsFalse(foo.IsValid("fdsa@"));

            // IsValid can be either a powerful regex OR a dummy test,
            // and by default it depends on System.ComponentModel.DataAnnotations.AppSettings.DisableRegEx
            // which ends up using BinaryCompatibility.Current.TargetsAtLeastFramework472 so for some reason
            // in 472 we are not using the regex anymore
            //
            // it can be forced, though with an app settings
            // dataAnnotations:dataTypeAttribute:disableRegEx = false
            //
            // since Umbraco is now 4.7.2+, the setting is required for the following tests to pass

            Assert.IsFalse(foo.IsValid("fdsa@fdsa"));
            Assert.IsFalse(foo.IsValid("fdsa@fdsa."));

        }
    }
}
