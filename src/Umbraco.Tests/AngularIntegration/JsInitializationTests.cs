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
            var result = JsInitialization.ParseMain("[World]");

            Assert.IsTrue(result.StartsWith(@"yepnope({

    load: [World],"));
        }
    }
}
