using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Manifest;
using Umbraco.Web.UI.JavaScript;

namespace Umbraco.Tests.AngularIntegration
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
});", result);
        }
    }
}
