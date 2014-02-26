using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.installer;

namespace Umbraco.Web.Install.Steps
{
    internal class DefaultUser : InstallerStep
    {
        public override string Alias
        {
            get { return "defaultUser"; }
        }

        public override string Name
        {
          get { return "Create User"; }
        }

       public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/defaultuser.ascx"; }
        }

        public override bool Completed()
        {
            var u = global::umbraco.BusinessLogic.User.GetUser(0);
            if (u.NoConsole || u.Disabled)
                return true;

            if (u.GetPassword() != "default")
                return true;


            return false;
        }
    }
}