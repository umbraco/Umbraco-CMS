using System;
using System.Collections.Generic;
using System.IO;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "Permissions", 0, "",
        PerformsAppRestart = true)]
    internal class FilePermissionsStep : InstallSetupStep<object>
    {
        public override InstallSetupResult Execute(object model)
        {
            // validate file permissions
            Dictionary<string, IEnumerable<string>> report;
            var permissionsOk = FilePermissionHelper.RunFilePermissionTestSuite(out report);

            if (permissionsOk == false)
                throw new InstallException("Permission check failed", "permissionsreport", new { errors = report });

            return null;
        }

        public override bool RequiresExecution(object model)
        {
            return true;
        }
    }
}
