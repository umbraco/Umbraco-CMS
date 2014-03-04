using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Web.Install;
using Umbraco.Web.Install.Controllers;
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Tests.Install
{
    [TestFixture]
    public class InstallHelperTests
    {

        [Test]
        public void Get_Steps()
        {
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());
            ApplicationContext.EnsureContext(appCtx, true);
            
            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                true);

            var helper = new InstallHelper(umbCtx, InstallStatusType.NewInstall);

            var steps = helper.GetSteps().ToArray();

            var expected = new[]
            {
                typeof (FilePermissionsStep), typeof (UserStep), typeof(MajorVersion7UpgradeReport), typeof (DatabaseConfigureStep), typeof (DatabaseInstallStep),
                typeof (DatabaseUpgradeStep), typeof (StarterKitDownloadStep), typeof (StarterKitInstallStep), typeof (StarterKitCleanupStep),
                typeof (SetUmbracoVersionStep)
            };

            Assert.AreEqual(expected.Length, steps.Length);

            for (int i = 0; i < expected.Length; i++)
            {
                var type = expected[i];
                Assert.AreEqual(type, steps[i].GetType());
            }
        }

        [Test]
        public void Get_Steps_New_Install()
        {
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());
            ApplicationContext.EnsureContext(appCtx, true);

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                true);

            var helper = new InstallHelper(umbCtx, InstallStatusType.NewInstall);

            var steps = helper.GetSteps().ToArray();
            
            //for new installs 2, don't require execution
            Assert.AreEqual(2, steps.Count(x => x.RequiresExecution() == false));

        }

        [Test]
        public void Get_Steps_Upgrade()
        {
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());
            ApplicationContext.EnsureContext(appCtx, true);
            appCtx.DatabaseContext = new DatabaseContext(Mock.Of<IDatabaseFactory>());

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                true);

            var helper = new InstallHelper(umbCtx, InstallStatusType.Upgrade);

            var steps = helper.GetSteps().ToArray();

            //for upgrades 4, don't require execution 
            Assert.AreEqual(4, steps.Count(x => x.RequiresExecution() == false));

        }

    }
}
