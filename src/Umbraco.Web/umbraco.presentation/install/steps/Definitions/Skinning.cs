using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.installer;
using umbraco.cms.businesslogic.packager;
using umbraco.cms.businesslogic.web;

namespace umbraco.presentation.install.steps.Definitions
{
    [Obsolete("This is no longer used and will be removed from the codebase in the future. These legacy installer step classes have been superceded by classes in the Umbraco.Web.Install.Steps namespace but are marked internal as they are not to be used in external code.")]
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
            if (String.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus) == false)
                return true;

            if (InstalledPackage.GetAllInstalledPackages().Count > 0)
                return true;
            
            if (Document.GetRootDocuments().Count() > 0)
                return true;
            

            return false;
        }
    }
}