using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.installer;

namespace umbraco.presentation.install.steps.Definitions
{
    public class FilePermissions : InstallerStep
    {
        public override string Alias
        {
            get { return "filepermissions"; }
        }

        public override string Name
        {
            get { return "Confirm permissions"; }
        }

        public override string UserControl
        {
            get { return IO.SystemDirectories.Install + "/steps/validatepermissions.ascx"; }
        }

        public override bool HideFromNavigation {
          get {
            return true;
          }
        }
        
        public override bool Completed()
        {
            return utills.FilePermissions.RunFilePermissionTestSuite();
        }
    }
}