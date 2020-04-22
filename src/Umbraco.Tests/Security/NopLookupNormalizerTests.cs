using System;
using NUnit.Framework;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Security
{
    public class NopLookupNormalizerTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void NormalizeName_When_Name_Null_Or_Whitespace_Expect_ArgumentNullException(string name)
        {
            var sut = new NopLookupNormalizer();

            Assert.Throws<ArgumentNullException>(() => sut.NormalizeName(name));
        }

        [Test]
        public void NormalizeName_Expect_Input_Returned()
        {
            var name = Guid.NewGuid().ToString();
            var sut = new NopLookupNormalizer();

            var normalizedName = sut.NormalizeName(name);

            Assert.AreEqual(name, normalizedName);
        }
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void NormalizeEmail_When_Name_Null_Or_Whitespace_Expect_ArgumentNullException(string email)
        {
            var sut = new NopLookupNormalizer();

            Assert.Throws<ArgumentNullException>(() => sut.NormalizeEmail(email));
        }

        [Test]
        public void NormalizeEmail_Expect_Input_Returned()
        {
            var name = $"{Guid.NewGuid()}@umbraco";
            var sut = new NopLookupNormalizer();

            var normalizedName = sut.NormalizeEmail(name);

            Assert.AreEqual(name, normalizedName);
        }
    }
}
