﻿using System.Threading.Tasks;
using Umbraco.Cms.Core.Install.Models;

namespace Umbraco.Cms.Infrastructure.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "UmbracoVersion", 50, "Installation is complete! Get ready to be redirected to your new CMS.",
        PerformsAppRestart = true)]
    public class CompleteInstallStep : InstallSetupStep<object>
    {
        private readonly InstallHelper _installHelper;

        public CompleteInstallStep(InstallHelper installHelper)
        {
            _installHelper = installHelper;
        }

        public override async Task<InstallSetupResult> ExecuteAsync(object model)
        {
            //reports the ended install
            await _installHelper.SetInstallStatusAsync(true, "");

            return null;
        }

        public override bool RequiresExecution(object model)
        {
            return true;
        }
    }
}
