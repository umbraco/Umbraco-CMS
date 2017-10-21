using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Web.TokenReplacers.Replacers;

namespace Umbraco.Tests.TokenReplacers
{
    [TestFixture]
    public class ParentNameTokenReplacerTests : TokenReplacerTests
    {
        [SetUp]
        public override void Setup()
        {
            MockContentService.Setup(x => x.GetById(It.Is<int>(y => y == 1000)))
                .Returns(new Content("Parent", -1, new ContentType(-1)));

            base.Setup();
        }


        [Test]
        public void ReplacesTokens()
        {
            var tokenReplacer = new ParentNameTokenReplacer(TokenReplacerContext);
            var model = GetModel("Created from parent with name: #parentname#");

            tokenReplacer.ReplaceTokens(model);

            Assert.AreEqual("Created from parent with name: Parent", GetTestPropertyValue(model));
        }
    }
}
