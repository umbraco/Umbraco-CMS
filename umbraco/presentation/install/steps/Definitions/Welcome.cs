using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.installer;

namespace umbraco.presentation.install.steps.Definitions
{
    public class Welcome : InstallerStep
    {
        public override string Alias
        {
            get { return "welcome"; }
        }

        public override string Name
        {
            get { return "Welcome"; }
        }

      
        public override string UserControl
        {
            get { return IO.SystemDirectories.Install + "/steps/welcome.ascx"; }
        }

        public override bool Completed()
        {
            return false;
        }

     }
}