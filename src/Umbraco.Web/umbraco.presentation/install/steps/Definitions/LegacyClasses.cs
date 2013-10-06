using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.installer;
using umbraco.cms.businesslogic.packager;
using umbraco.cms.businesslogic.web;

namespace umbraco.presentation.install.steps.Definitions
{

    [Obsolete("This is no longer used and will be removed from the codebase in the future. These legacy installer step classes have been superceded by classes in the Umbraco.Web.Install.Steps namespace but are marked internal as they are not to be used in external code.")]
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
            get { return SystemDirectories.Install + "/steps/welcome.ascx"; }
        }

        public override bool Completed()
        {
            return false;
        }

    }

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

        public override bool HideFromNavigation
        {
            get
            {
                return true;
            }
        }

        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/skinning.ascx"; }
        }

        public override bool Completed()
        {
            //this is always completed, we just want to be able to hook into directly after finishing web pi 
            return true;
        }
    }

    [Obsolete("This is no longer used and will be removed from the codebase in the future. These legacy installer step classes have been superceded by classes in the Umbraco.Web.Install.Steps namespace but are marked internal as they are not to be used in external code.")]
    public class TheEnd : InstallerStep
    {
        public override string Alias
        {
            get { return "theend"; }
        }

        public override string Name
        {
            get { return "You’re Done"; }
        }

        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/theend.ascx"; }
        }

        public override bool Completed()
        {
            return false;
        }
    }

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
            get { return SystemDirectories.Install + "/steps/skinning.ascx"; }
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

    [Obsolete("This is no longer used and will be removed from the codebase in the future. These legacy installer step classes have been superceded by classes in the Umbraco.Web.Install.Steps namespace but are marked internal as they are not to be used in external code.")]
    public class License : InstallerStep
    {
        public override string Alias
        {
            get { return "license"; }
        }

        public override string Name
        {
            get { return "License"; }
        }



        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/license.ascx"; }
        }

        public override bool Completed()
        {
            return false;
        }


    }

    [Obsolete("This is no longer used and will be removed from the codebase in the future. ")]
    public class InstallerControl : System.Web.UI.UserControl
    {
        public void NextStep()
        {
            _default p = (_default)this.Page;
            p.GotoNextStep(helper.Request("installStep"));
        }
    }

    [Obsolete("This is no longer used and will be removed from the codebase in the future. These legacy installer step classes have been superceded by classes in the Umbraco.Web.Install.Steps namespace but are marked internal as they are not to be used in external code.")]
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
            get { return SystemDirectories.Install + "/steps/validatepermissions.ascx"; }
        }

        public override bool HideFromNavigation
        {
            get
            {
                return true;
            }
        }

        public override bool Completed()
        {
            return utills.FilePermissions.RunFilePermissionTestSuite();
        }
    }

    [Obsolete("This is no longer used and will be removed from the codebase in the future. These legacy installer step classes have been superceded by classes in the Umbraco.Web.Install.Steps namespace but are marked internal as they are not to be used in external code.")]
    public class DefaultUser : InstallerStep
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
            BusinessLogic.User u = BusinessLogic.User.GetUser(0);
            if (u.NoConsole || u.Disabled)
                return true;

            if (u.GetPassword() != "default")
                return true;


            return false;
        }
    }

    [Obsolete("This is no longer used and will be removed from the codebase in the future. These legacy installer step classes have been superceded by classes in the Umbraco.Web.Install.Steps namespace but are marked internal as they are not to be used in external code.")]
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
            get { return true; }
        }

        //here we determine if the installer should skip this step...
        public override bool Completed()
        {
            // Fresh installs don't have a version number so this step cannot be complete yet
            if (string.IsNullOrEmpty(Umbraco.Core.Configuration.GlobalSettings.ConfigurationStatus))
            {
                //Even though the ConfigurationStatus is blank we try to determine the version if we can connect to the database
                var result = ApplicationContext.Current.DatabaseContext.ValidateDatabaseSchema();
                var determinedVersion = result.DetermineInstalledVersion();
                if (determinedVersion.Equals(new Version(0, 0, 0)))
                    return false;

                return UmbracoVersion.Current < determinedVersion;
            }

            var configuredVersion = new Version(Umbraco.Core.Configuration.GlobalSettings.ConfigurationStatus);
            var targetVersion = UmbracoVersion.Current;

            return targetVersion < configuredVersion;
        }
    }
}