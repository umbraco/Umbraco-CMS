using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.installer;
using umbraco.cms.businesslogic.packager;
using umbraco.cms.businesslogic.web;

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
          get { return "Starter Kit"; }
        }

     
        public override string UserControl
        {
            get { return IO.SystemDirectories.Install + "/steps/skinning.ascx"; }
        }

        public override bool Completed()
        {
            if (!String.IsNullOrEmpty(GlobalSettings.ConfigurationStatus.Trim()))
                return true;

            if (InstalledPackage.GetAllInstalledPackages().Count > 0)
                return true;
            
            if (Document.GetRootDocuments().Count() > 0)
                return true;
            

            return false;
        }
    }
}