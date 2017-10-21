using System.Linq;
using NUnit.Framework;
using Umbraco.Web.TokenReplacers.Replacers;

namespace Umbraco.Tests.TokenReplacers
{
    [TestFixture]
    public class NameTokenReplacerTests : TokenReplacerTests
    {
        [Test]
        public void ReplacesTokens()
        {
            var tokenReplacer = new NameTokenReplacer(TokenReplacerContext);
            var model = GetModel("Hello #name#!", contentName: "World");

            tokenReplacer.ReplaceTokens(model);

            Assert.AreEqual("Hello World!", GetTestPropertyValue(model));
        }
    }
}
