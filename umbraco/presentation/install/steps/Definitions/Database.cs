using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.installer;
using umbraco.IO;
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
            get { return "Database configuration"; }
        }

        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/database.ascx"; }
        }

        public override bool HideNextButtonUntillCompleted
        {
            get
            {
                return false;
            }
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
            bool retval = false;
            try
            {
                IInstallerUtility m_Installer = BusinessLogic.Application.SqlHelper.Utility.CreateInstaller();
                retval = m_Installer.IsLatestVersion;
                m_Installer = null;
            } catch {
                // this step might fail due to missing connectionstring
                return false;
            }

            return retval;
        }

        
    }
}