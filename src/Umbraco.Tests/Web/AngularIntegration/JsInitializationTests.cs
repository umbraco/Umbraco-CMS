using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Web.UI.JavaScript;

namespace Umbraco.Tests.Web.AngularIntegration
{
    [TestFixture]
    public class JsInitializationTests
    {

        [Test]
        public void Get_Default_Init()
        {
            var init = JsInitialization.GetDefaultInitialization();
            Assert.IsTrue(init.Any());
        }

        [Test]
        public void Parse_Main()
        {
            var result = JsInitialization.ParseMain(new[] {"[World]", "Hello" });

            Assert.AreEqual(@"LazyLoad.js([World], function () {
    //we need to set the legacy UmbClientMgr path
    UmbClientMgr.setUmbracoPath('Hello');

    jQuery(document).ready(function () {

        angular.bootstrap(document, ['umbraco']);

    });
});".StripWhitespace(), result.StripWhitespace());
        }
    }
}
