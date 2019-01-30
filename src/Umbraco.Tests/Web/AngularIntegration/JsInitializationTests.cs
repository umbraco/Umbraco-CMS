using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Web.JavaScript;

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
            var result = JsInitialization.WriteScript("[World]", "Hello", "Blah");

            Assert.AreEqual(@"LazyLoad.js([World], function () {
    //we need to set the legacy UmbClientMgr path
    if ((typeof UmbClientMgr) !== ""undefined"") {
        UmbClientMgr.setUmbracoPath('Hello');
    }

    jQuery(document).ready(function () {

        angular.bootstrap(document, ['Blah']);

    });
});".StripWhitespace(), result.StripWhitespace());
        }
    }
}
