using System;
using NUnit.Framework;
using Umbraco.Core.BackOffice;

namespace Umbraco.Tests.Integration.Umbraco.Web.Backoffice
{
    public class BackOfficeLookupNormalizerTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void NormalizeName_When_Name_Null_Or_Whitespace_Expect_Same_Returned(string name)
        {
            var sut = new BackOfficeLookupNormalizer();

            var normalizedName = sut.NormalizeName(name);

            Assert.AreEqual(name, normalizedName);
        }

        [Test]
        public void NormalizeName_Expect_Input_Returned()
        {
            var name = Guid.NewGuid().ToString();
            var sut = new BackOfficeLookupNormalizer();

            var normalizedName = sut.NormalizeName(name);

            Assert.AreEqual(name, normalizedName);
        }
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void NormalizeEmail_When_Name_Null_Or_Whitespace_Expect_Same_Returned(string email)
        {
            var sut = new BackOfficeLookupNormalizer();

            var normalizedEmail = sut.NormalizeEmail(email);

            Assert.AreEqual(email, normalizedEmail);
        }

        [Test]
        public void NormalizeEmail_Expect_Input_Returned()
        {
            var email = $"{Guid.NewGuid()}@umbraco";
            var sut = new BackOfficeLookupNormalizer();

            var normalizedEmail = sut.NormalizeEmail(email);

            Assert.AreEqual(email, normalizedEmail);
        }
    }
}
