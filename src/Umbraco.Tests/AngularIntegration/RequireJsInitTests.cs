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
    public class RequireJsInitTests
    {

        [Test]
        public void Get_Default_Config()
        {
            var config = RequireJsInit.GetDefaultConfig();
            var paths = config.Properties().SingleOrDefault(x => x.Name == "paths");
            var shim = config.Properties().SingleOrDefault(x => x.Name == "shim");
            Assert.IsNotNull(paths);
            Assert.AreEqual(typeof(JProperty), paths.GetType());
            Assert.IsNotNull(shim);
            Assert.AreEqual(typeof(JProperty), shim.GetType());
        }

        [Test]
        public void Get_Default_Init()
        {
            var init = RequireJsInit.GetDefaultInitialization();
            Assert.IsTrue(init.Any());
        }

        [Test]
        public void Parse_Main()
        {
            var result = RequireJsInit.ParseMain("{Hello}", "[World]");

            Assert.IsTrue(result.StartsWith("require.config({Hello});"));
            Assert.IsTrue(result.Contains("require([World]"));
        }

    }
}
