using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using Umbraco.Core;
using Umbraco.Core.Auditing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.packager.repositories
{
    [Obsolete("This should not be used and will be removed in future Umbraco versions")]
    public class Repository : DisposableObject
    {        
        public string Guid { get; private set; }

        public string Name { get; private set; }

        public string RepositoryUrl { get; private set; }

        public string WebserviceUrl { get; private set; }


        public RepositoryWebservice Webservice
        {
            get
            {
                var repo = new RepositoryWebservice(WebserviceUrl);
                return repo;
            }
        }

        public SubmitStatus SubmitPackage(string authorGuid, PackageInstance package, byte[] doc)
        {

            string packageName = package.Name;
            string packageGuid = package.PackageGuid;
            string description = package.Readme;
            string packageFile = package.PackagePath;


            System.IO.FileStream fs1 = null;

            try
            {

                byte[] pack = new byte[0];
                fs1 = System.IO.File.Open(IOHelper.MapPath(packageFile), FileMode.Open, FileAccess.Read);
                pack = new byte[fs1.Length];
                fs1.Read(pack, 0, (int) fs1.Length);
                fs1.Close();
                fs1 = null;

                byte[] thumb = new byte[0]; //todo upload thumbnail... 

                return Webservice.SubmitPackage(Guid, authorGuid, packageGuid, pack, doc, thumb, packageName, "", "", description);
            }
            catch (Exception ex)
            {
                LogHelper.Error<Repository>("An error occurred in SubmitPackage", ex);

                return SubmitStatus.Error;
            }
        }

        public static List<Repository> getAll()
        {

            var repositories = new List<Repository>();

            foreach (var r in UmbracoConfig.For.UmbracoSettings().PackageRepositories.Repositories)
            {
                var repository = new Repository
                {
                    Guid = r.Id.ToString(),
                    Name = r.Name
                };

                repository.RepositoryUrl = r.RepositoryUrl;
                repository.WebserviceUrl = repository.RepositoryUrl.Trim('/') + "/" + r.WebServiceUrl.Trim('/');
                if (r.HasCustomWebServiceUrl)
                {
                    string wsUrl = r.WebServiceUrl;

                    if (wsUrl.Contains("://"))
                    {
                        repository.WebserviceUrl = r.WebServiceUrl;
                    }
                    else
                    {
                        repository.WebserviceUrl = repository.RepositoryUrl.Trim('/') + "/" + wsUrl.Trim('/');
                    }
                }

                repositories.Add(repository);
            }

            return repositories;
        }

        public static Repository getByGuid(string repositoryGuid)
        {
            Guid id;
            if (System.Guid.TryParse(repositoryGuid, out id) == false)
            {
                throw new FormatException("The repositoryGuid is not a valid GUID");
            }

            var found = UmbracoConfig.For.UmbracoSettings().PackageRepositories.Repositories.FirstOrDefault(x => x.Id == id);
            if (found == null)
            {
                return null;
            }
            
            var repository = new Repository
            {
                Guid = found.Id.ToString(),
                Name = found.Name
            };

            repository.RepositoryUrl = found.RepositoryUrl;
            repository.WebserviceUrl = repository.RepositoryUrl.Trim('/') + "/" + found.WebServiceUrl.Trim('/');

            if (found.HasCustomWebServiceUrl)
            {
                string wsUrl = found.WebServiceUrl;

                if (wsUrl.Contains("://"))
                {
                    repository.WebserviceUrl = found.WebServiceUrl;
                }
                else
                {
                    repository.WebserviceUrl = repository.RepositoryUrl.Trim('/') + "/" + wsUrl.Trim('/');
                }
            }

            return repository;
        }

        

        //shortcut method to download pack from repo and place it on the server...
        public string fetch(string packageGuid)
        {
            return fetch(packageGuid, string.Empty);
        }

        public string fetch(string packageGuid, int userId)
        {
            // log
            Audit.Add(AuditTypes.PackagerInstall,
                                    string.Format("Package {0} fetched from {1}", packageGuid, this.Guid),
                                    userId, -1);
            return fetch(packageGuid);
        }

        /// <summary>
        /// Used to get the correct package file from the repo for the current umbraco version
        /// </summary>
        /// <param name="packageGuid"></param>
        /// <param name="userId"></param>
        /// <param name="currentUmbracoVersion"></param>
        /// <returns></returns>
        public string GetPackageFile(string packageGuid, int userId, System.Version currentUmbracoVersion)
        {
            // log
            Audit.Add(AuditTypes.PackagerInstall,
                string.Format("Package {0} fetched from {1}", packageGuid, this.Guid),
                userId, -1);

            var fileByteArray = Webservice.GetPackageFile(packageGuid, currentUmbracoVersion.ToString(3));

            //successfull 
            if (fileByteArray.Length > 0)
            {
                // Check for package directory
                if (Directory.Exists(IOHelper.MapPath(Settings.PackagerRoot)) == false)
                    Directory.CreateDirectory(IOHelper.MapPath(Settings.PackagerRoot));

                using (var fs1 = new FileStream(IOHelper.MapPath(Settings.PackagerRoot + Path.DirectorySeparatorChar + packageGuid + ".umb"), FileMode.Create))
                {
                    fs1.Write(fileByteArray, 0, fileByteArray.Length);
                    fs1.Close();
                    return "packages\\" + packageGuid + ".umb";
                }
            }
            
            return "";
        }

        public bool HasConnection()
        {

            string strServer = this.RepositoryUrl;

            try
            {

                HttpWebRequest reqFP = (HttpWebRequest) HttpWebRequest.Create(strServer);
                HttpWebResponse rspFP = (HttpWebResponse) reqFP.GetResponse();

                if (HttpStatusCode.OK == rspFP.StatusCode)
                {

                    // HTTP = 200 - Internet connection available, server online
                    rspFP.Close();

                    return true;

                }
                else
                {

                    // Other status - Server or connection not available

                    rspFP.Close();

                    return false;

                }

            }
            catch (WebException)
            {

                // Exception - connection not available

                return false;

            }
        }
        

        /// <summary>
        /// This goes and fetches the Byte array for the package from OUR, but it's pretty strange
        /// </summary>
        /// <param name="packageGuid">
        /// The package ID for the package file to be returned
        /// </param>
        /// <param name="key">
        /// This is a strange Umbraco version parameter - but it's not really an umbraco version, it's a special/odd version format like Version41
        /// but it's actually not used for the 7.5+ package installs so it's obsolete/unused.
        /// </param>
        /// <returns></returns>
        public string fetch(string packageGuid, string key)
        {

            byte[] fileByteArray = new byte[0];

            if (key == string.Empty)
            {
                //SD: this is odd, not sure why it returns a different package depending on the legacy xml schema but I'll leave it for now
                if (UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
                    fileByteArray = Webservice.fetchPackage(packageGuid);
                else
                {
                    fileByteArray = Webservice.fetchPackageByVersion(packageGuid, Version.Version41);
                }
            }
            else
            {
                fileByteArray = Webservice.fetchProtectedPackage(packageGuid, key);
            }

            //successfull 
            if (fileByteArray.Length > 0)
            {

                // Check for package directory
                if (Directory.Exists(IOHelper.MapPath(Settings.PackagerRoot)) == false)
                    Directory.CreateDirectory(IOHelper.MapPath(Settings.PackagerRoot));

                using (var fs1 = new FileStream(IOHelper.MapPath(Settings.PackagerRoot + Path.DirectorySeparatorChar + packageGuid + ".umb"), FileMode.Create))
                {
                    fs1.Write(fileByteArray, 0, fileByteArray.Length);
                    fs1.Close();
                    return "packages\\" + packageGuid + ".umb";
                }
            }

            // log

            return "";
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            Webservice.Dispose();
        }
    }
}
