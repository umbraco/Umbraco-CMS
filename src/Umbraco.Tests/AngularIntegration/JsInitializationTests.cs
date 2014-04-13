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
            var noCache = Resources.JsNoCache;
            noCache = noCache.Replace("##rnd##", "(new Date).getTime()");
            var result = JsInitialization.ParseMain(new[] { noCache, "[World]", "Hello" });

            Assert.AreEqual(noCache + @"
yepnope({
    load: [
         'lib/jquery/jquery-2.0.3.min.js',
         'lib/angular/1.1.5/angular.min.js',
         'lib/underscore/underscore.js',
    ],
    complete: function () {
        yepnope({
            load: [World],
            complete: function () {

                //we need to set the legacy UmbClientMgr path
                UmbClientMgr.setUmbracoPath('Hello');

                jQuery(document).ready(function () {
                    angular.bootstrap(document, ['umbraco']);
                });
            }
        });
    }
});", result);
        }
    }
}
