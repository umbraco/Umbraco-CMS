using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep("StarterKitInstall", 7, "Installing a starter website to help you get off to a great start")]
    internal class StarterKitInstallStep : InstallSetupStep<object>
    {
        private readonly InstallStatusType _status;
        private readonly ApplicationContext _applicationContext;
        private readonly HttpContextBase _httContext;

        public StarterKitInstallStep(InstallStatusType status, ApplicationContext applicationContext, HttpContextBase httContext)
        {
            _status = status;
            _applicationContext = applicationContext;
            _httContext = httContext;
        }


        public override InstallSetupResult Execute(object model)
        {
            if (_status != InstallStatusType.NewInstall) return null;

            var installSteps = InstallStatusTracker.GetStatus();            
            //this step relies on the preious one completed - because it has stored some information we need
            if (installSteps.Any(x => x.Key == "StarterKitDownload") == false)            
            {
                throw new InvalidOperationException("Could not find previous step: StarterKitDownload of the installation, package install cannot continue");
            }
            var previousStep = installSteps["StarterKitDownload"];
            var manifestId = Convert.ToInt32(previousStep.AdditionalData["manifestId"]);
            var packageFile = (string)previousStep.AdditionalData["packageFile"];

            InstallBusinessLogic(manifestId, packageFile);

            _applicationContext.RestartApplicationPool(_httContext);

            return null;
        }

        private void InstallBusinessLogic(int manifestId, string packageFile)
        {
            packageFile = HttpUtility.UrlDecode(packageFile);
            var installer = new global::umbraco.cms.businesslogic.packager.Installer();
            installer.LoadConfig(packageFile);
            installer.InstallBusinessLogic(manifestId, packageFile);            
        }

        public override bool RequiresExecution()
        {
            return _status == InstallStatusType.NewInstall;
        }
    }
}