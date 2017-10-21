using System;
using NUnit.Framework;
using Umbraco.Web.TokenReplacers.Replacers;

namespace Umbraco.Tests.TokenReplacers
{
    [TestFixture]
    public class DateTimeTokenReplacerTests : TokenReplacerTests
    {
        [Test]
        public void ReplacesTokensWithFormat()
        {
            var tokenReplacer = new DateTimeTokenReplacer(TokenReplacerContext);
            var model = GetModel("Page created on #datetime:d-MMM-yyyy#.");
            var testDate = DateTime.Now;
            tokenReplacer.ReplaceTokens(model);

            Assert.AreEqual(string.Format("Page created on {0}.", testDate.ToString("d-MMM-yyyy")), GetTestPropertyValue(model));
        }

        [Test]
        public void ReplacesTokensWithoutFormat()
        {
            var tokenReplacer = new DateTimeTokenReplacer(TokenReplacerContext);
            var model = GetModel("Page created on #datetime#.");
            var testDate = DateTime.Now;
            tokenReplacer.ReplaceTokens(model);

            Assert.AreEqual(string.Format("Page created on {0}.", testDate), GetTestPropertyValue(model));
        }
    }
}
