using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Persistence;
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    internal class InstallHelper
    {
        private readonly UmbracoContext _umbContext;
        private InstallationType? _installationType;

        internal InstallHelper(UmbracoContext umbContext)
        {
            _umbContext = umbContext;
        }


        /// <summary>
        /// Get the installer steps
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The step order returned here is how they will appear on the front-end if they have views assigned
        /// </remarks>
        public IEnumerable<InstallSetupStep> GetAllSteps()
        {
            return new List<InstallSetupStep>
            {                
                new NewInstallStep(_umbContext.Application),
                new UpgradeStep(),
                new FilePermissionsStep(),
                new MajorVersion7UpgradeReport(_umbContext.Application),
                new DatabaseConfigureStep(_umbContext.Application),
                new DatabaseInstallStep(_umbContext.Application),
                new DatabaseUpgradeStep(_umbContext.Application),
                new StarterKitDownloadStep(_umbContext.Application),
                new StarterKitInstallStep(_umbContext.Application, _umbContext.HttpContext),
                new StarterKitCleanupStep(_umbContext.Application),
                new SetUmbracoVersionStep(_umbContext.Application, _umbContext.HttpContext),
            };
        }

        /// <summary>
        /// Returns the steps that are used only for the current installation type
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InstallSetupStep> GetStepsForCurrentInstallType()
        {
            return GetAllSteps().Where(x => x.InstallTypeTarget.HasFlag(GetInstallationType()));
        }

        public InstallationType GetInstallationType()
        {
            return _installationType ?? (_installationType = IsBrandNewInstall ? InstallationType.NewInstall : InstallationType.Upgrade).Value;
        }

        internal void DeleteLegacyInstaller()
        {
            if (Directory.Exists(IOHelper.MapPath(SystemDirectories.Install)))
            {
                if (Directory.Exists(IOHelper.MapPath("~/app_data/temp/install_backup")))
                {
                    //this means the backup already exists with files but there's no files in it, so we'll delete the backup and re-run it
                    if (Directory.GetFiles(IOHelper.MapPath("~/app_data/temp/install_backup")).Any() == false)
                    {
                        Directory.Delete(IOHelper.MapPath("~/app_data/temp/install_backup"), true);
                        Directory.Move(IOHelper.MapPath(SystemDirectories.Install), IOHelper.MapPath("~/app_data/temp/install_backup"));
                    }
                }
                else
                {
                    Directory.Move(IOHelper.MapPath(SystemDirectories.Install), IOHelper.MapPath("~/app_data/temp/install_backup"));
                }
            }                

            if (Directory.Exists(IOHelper.MapPath("~/Areas/UmbracoInstall")))
            {                
                Directory.Delete(IOHelper.MapPath("~/Areas/UmbracoInstall"), true);
            }
                
        }

        /// <summary>
        /// Checks if this is a brand new install meaning that there is no configured version and there is no configured database connection
        /// </summary>
        private bool IsBrandNewInstall
        {
            get
            {
                var databaseSettings = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName];
                if (GlobalSettings.ConfigurationStatus.IsNullOrWhiteSpace()
                    && _umbContext.Application.DatabaseContext.IsConnectionStringConfigured(databaseSettings) == false)
                {
                    //no version or conn string configured, must be a brand new install
                    return true;
                }

                //now we have to check if this is really a new install, the db might be configured and might contain data

                if (_umbContext.Application.DatabaseContext.IsConnectionStringConfigured(databaseSettings) == false
                    || _umbContext.Application.DatabaseContext.IsDatabaseConfigured == false)
                {
                    return true;
                }

                //check if we have the default user configured already
                var result = _umbContext.Application.DatabaseContext.Database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM umbracoUser WHERE id=0 AND userPassword='default'");
                if (result == 1)
                {
                    //the user has not been configured
                    return true;
                }

                //                //check if there are any content types configured, if there isn't then we will consider this a new install
                //                result = _umbContext.Application.DatabaseContext.Database.ExecuteScalar<int>(
                //                    @"SELECT COUNT(*) FROM cmsContentType 
                //                        INNER JOIN umbracoNode ON cmsContentType.nodeId = umbracoNode.id
                //                        WHERE umbracoNode.nodeObjectType = @contentType", new {contentType = Constants.ObjectTypes.DocumentType});
                //                if (result == 0)
                //                {
                //                    //no content types have been created
                //                    return true;
                //                }

                return false;
            }
        }
    }
}