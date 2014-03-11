using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.UI.Install.Steps.Skinning;
using Umbraco.Web.UI.Pages;
using System.IO;
using umbraco.cms.businesslogic.packager;

namespace Umbraco.Web.UI.Umbraco.Developer.Packages
{
    

    public partial class StarterKits : UmbracoEnsuredPage
    {
        private const string RepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        protected void Page_Load(object sender, EventArgs e)
        {
            //check if a starter kit is already isntalled

            var installed = InstalledPackage.GetAllInstalledPackages();

            if (installed.Count == 0)
            {
                ShowStarterKits();
                return;
            }
            
            var repo = global::umbraco.cms.businesslogic.packager.repositories.Repository.getByGuid(RepoGuid);
            if (repo.HasConnection())
            {
                try
                {
                    var kits = repo.Webservice.StarterKits();
                    var kitIds = kits.Select(x => x.RepoGuid).ToArray();

                    //if a starter kit is already installed show finish
                    if (installed.Any(x => kitIds.Contains(Guid.Parse(x.Data.PackageGuid))))
                    {
                        StarterKitNotInstalled.Visible = false;
                        installationCompleted.Visible = true;
                    }
                    else
                    {
                        ShowStarterKits();    
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<StarterKits>("Cannot connect to package repository", ex);
                    InstallationDirectoryNotAvailable.Visible = true;
                    StarterKitNotInstalled.Visible = false;
                }
            }
            else
            {
                InstallationDirectoryNotAvailable.Visible = true;
                StarterKitNotInstalled.Visible = false;
            }            
        }

        private void ShowStarterKits()
        {
            if (Directory.Exists(Server.MapPath(GlobalSettings.Path.EnsureEndsWith('/') + "install/Legacy")) == false)
            {
                InstallationDirectoryNotAvailable.Visible = true;
                StarterKitNotInstalled.Visible = false;
            
                return;
            }


            var starterkitsctrl = (LoadStarterKits)LoadControl(GlobalSettings.Path.EnsureEndsWith('/') + "install/Legacy/loadStarterKits.ascx");
            
            ph_starterkits.Controls.Add(starterkitsctrl);

            StarterKitNotInstalled.Visible = true;

        }
        
    }
}