using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.installer;
using umbraco.BusinessLogic;

namespace umbraco.presentation.install.steps.Definitions
{
    [Obsolete("This is no longer used and will be removed from the codebase in the future. These legacy installer step classes have been superceded by classes in the Umbraco.Web.Install.Steps namespace but are marked internal as they are not to be used in external code.")]
    public class WebPi : InstallerStep
    {
        public override string Alias
        {
            get { return "webpi"; }
        }

        public override string Name
        {
            get { return "Hi " + new User(0).Name + " you are running umbraco"; }
        }

        public override bool HideFromNavigation {
          get {
            return true;
          }
        }

        public override string UserControl
        {
            get { return IO.SystemDirectories.Install + "/steps/skinning.ascx"; }
        }

        public override bool Completed()
        {
            //this is always completed, we just want to be able to hook into directly after finishing web pi 
            return true;
        }
    }
}