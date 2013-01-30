using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.installer;

using umbraco.DataLayer.Utility.Installer;
using umbraco.DataLayer;

namespace umbraco.presentation.install.steps.Definitions
{
    public class Database : InstallerStep
    {
        public override string Alias
        {
            get { return "database"; }
        }

        public override string Name
        {
            get { return "Database"; }
        }

        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/database.ascx"; }
        }


        public override bool MoveToNextStepAutomaticly
        {
            get
            {
                return true;
            }
        }

        //here we determine if the installer should skip this step...
        public override bool Completed()
        {
            bool retval;
            try
            {
                var installer = BusinessLogic.Application.SqlHelper.Utility.CreateInstaller();
                retval = installer.IsLatestVersion;
            }
            catch
            {
                // this step might fail due to missing connectionstring
                return false;
            }

            return retval;
        }


    }
}