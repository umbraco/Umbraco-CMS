using System.Linq;
using NUnit.Framework;
using Umbraco.Web.TokenReplacers.Replacers;

namespace Umbraco.Tests.TokenReplacers
{
    [TestFixture]
    public class ParentIdTokenReplacerTests : TokenReplacerTests
    {
        [Test]
        public void ReplacesTokens()
        {
            var tokenReplacer = new ParentIdTokenReplacer(TokenReplacerContext);
            var model = GetModel("Created from parent with id: #parentid#");

            tokenReplacer.ReplaceTokens(model);

            Assert.AreEqual("Created from parent with id: 1000", model.Properties.Single(x => x.Alias == "testProperty").Value.ToString());
        }
    }
}
