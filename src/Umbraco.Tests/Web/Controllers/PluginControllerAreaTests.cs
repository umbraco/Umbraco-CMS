using System;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace Umbraco.Tests.Web.Controllers
{
    [TestFixture]
    public class PluginControllerAreaTests : BaseWebTest
    {

        [Test]
        public void Ensure_Same_Area1()
        {
            Assert.Throws<InvalidOperationException>(() =>
                                                     new PluginControllerArea(TestObjects.GetGlobalSettings(),
                                                         new PluginControllerMetadata[]
                                                         {
                                                            PluginController.GetMetadata(typeof(Plugin1Controller)),
                                                            PluginController.GetMetadata(typeof(Plugin2Controller)),
                                                            PluginController.GetMetadata(typeof(Plugin3Controller)) //not same area
                                                         }));
        }

        [Test]
        public void Ensure_Same_Area3()
        {
            Assert.Throws<InvalidOperationException>(() =>
                                                     new PluginControllerArea(TestObjects.GetGlobalSettings(),
                                                         new PluginControllerMetadata[]
                                                         {
                                                            PluginController.GetMetadata(typeof(Plugin1Controller)),
                                                            PluginController.GetMetadata(typeof(Plugin2Controller)),
                                                            PluginController.GetMetadata(typeof(Plugin4Controller)) //no area assigned
                                                         }));
        }

        [Test]
        public void Ensure_Same_Area2()
        {
            var area = new PluginControllerArea(TestObjects.GetGlobalSettings(),
                new PluginControllerMetadata[]
                {
                    PluginController.GetMetadata(typeof(Plugin1Controller)),
                    PluginController.GetMetadata(typeof(Plugin2Controller))
                });
            Assert.Pass();
        }

        #region Test classes

        [PluginController("Area1")]
        public class Plugin1Controller : PluginController
        {
            public Plugin1Controller(UmbracoHelper umbracoHelper)
                : base(umbracoHelper, null, null, null, null, null)
            {
            }
        }

        [PluginController("Area1")]
        public class Plugin2Controller : PluginController
        {
            public Plugin2Controller(UmbracoHelper umbracoHelper)
                : base(umbracoHelper, null, null, null, null, null)
            {
            }
        }

        [PluginController("Area2")]
        public class Plugin3Controller : PluginController
        {
            public Plugin3Controller(UmbracoHelper umbracoHelper)
                : base(umbracoHelper, null, null, null, null, null)
            {
            }
        }

        public class Plugin4Controller : PluginController
        {
            public Plugin4Controller(UmbracoHelper umbracoHelper)
                : base(umbracoHelper, null, null, null, null, null)
            {
            }
        }

        #endregion

    }
}
