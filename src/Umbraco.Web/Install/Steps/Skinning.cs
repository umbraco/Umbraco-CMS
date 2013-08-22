using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.installer;
using umbraco.cms.businesslogic.packager;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Install.Steps
{
    internal class Skinning : InstallerStep
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
            get { return SystemDirectories.Install + "/steps/StarterKits.ascx"; }
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