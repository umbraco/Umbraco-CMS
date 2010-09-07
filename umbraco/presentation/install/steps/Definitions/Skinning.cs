using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.installer;
using umbraco.cms.businesslogic.packager;

namespace umbraco.presentation.install.steps.Definitions
{
    public class Skinning : InstallerStep
    {
        public override string Alias
        {
            get { return "skinning"; }
        }

        public override string Name
        {
            get { return "Configure your site"; }
        }

        public override bool HideNextButtonUntillCompleted
        {
            get
            {
                return true;
            }
        }

        public override string UserControl
        {
            get { return IO.SystemDirectories.Install + "/steps/skinning.ascx"; }
        }

        public override bool Completed()
        {
            if (InstalledPackage.GetAllInstalledPackages().Count > 0)
                return true;
            else
                return false;
        }
    }
}