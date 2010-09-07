using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.installer;

namespace umbraco.presentation.install.steps.Definitions
{
    public class DefaultUser : InstallerStep
    {
        public override string Alias
        {
            get { return "defaultUser"; }
        }

        public override string Name
        {
            get { return "Configure the administrator password"; }
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
            get { return IO.SystemDirectories.Install + "/steps/defaultuser.ascx"; }
        }

        public override bool Completed()
        {
            BusinessLogic.User u = BusinessLogic.User.GetUser(0);
            if (u.NoConsole || u.Disabled)
                return true;

            if (u.GetPassword() != "default")
                return true;


            return false;
        }
    }
}