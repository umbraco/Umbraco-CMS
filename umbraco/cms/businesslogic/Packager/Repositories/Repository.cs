using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using umbraco.IO;

namespace umbraco.cms.businesslogic.packager.repositories {
    public class Repository {

        private string m_guid;
        private string m_name;

        //this is our standard urls.
        private string m_repositoryUrl = "http://packages.umbraco.org";
        private string m_webserviceUrl = "/umbraco/webservices/api/repository.asmx";


        private static XmlNode m_repositories = UmbracoSettings.Repositories;

        public string Guid {
            get { return m_guid; }
        }

        public string Name {
            get { return m_name; }
        }

        public string RepositoryUrl {
            get { return m_repositoryUrl; }
        }

        public string WebserviceUrl {
            get { return m_webserviceUrl; }
            set { m_webserviceUrl = value; }
        }


        public RepositoryWebservice Webservice {
            get {

                if (!m_webserviceUrl.Contains("://")) {
                    m_webserviceUrl = m_repositoryUrl.Trim('/') + "/" + m_webserviceUrl.Trim('/');
                }

                RepositoryWebservice repo = new RepositoryWebservice(m_webserviceUrl);
                return repo;
            }
        }

        public SubmitStatus SubmitPackage(string authorGuid, PackageInstance package, byte[] doc) {

            string packageName = package.Name;
            string packageGuid = package.PackageGuid;
            string description = package.Readme;
            string packageFile = package.PackagePath;


            System.IO.FileStream fs1 = null;

            try {

                byte[] pack = new byte[0];
                fs1 = System.IO.File.Open(IOHelper.MapPath(packageFile), FileMode.Open, FileAccess.Read);
                pack = new byte[fs1.Length];
                fs1.Read(pack, 0, (int)fs1.Length);
                fs1.Close();
                fs1 = null;

                byte[] thumb = new byte[0]; //todo upload thumbnail... 

                return Webservice.SubmitPackage(m_guid, authorGuid, packageGuid, pack, doc, thumb, packageName, "", "", description);
            } catch (Exception ex) {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());

                return SubmitStatus.Error;
            }
        }

        public static List<Repository> getAll() {

            List<Repository> repositories = new List<Repository>();

            XmlNodeList repositoriesXml = m_repositories.SelectNodes("./repository");

            foreach (XmlNode repositoryXml in repositoriesXml) {
                Repository repository = new Repository();
                repository.m_guid = repositoryXml.Attributes["guid"].Value;
                repository.m_name = repositoryXml.Attributes["name"].Value;

                if (repositoryXml.Attributes["repositoryurl"] != null) {
                    repository.m_repositoryUrl = repositoryXml.Attributes["repositoryurl"].Value;
                    repository.m_webserviceUrl = repository.m_repositoryUrl.Trim('/') + "/" + repository.m_webserviceUrl.Trim('/');
                }

                if (repositoryXml.Attributes["webserviceurl"] != null) {
                    string wsUrl = repositoryXml.Attributes["webserviceurl"].Value;

                    if (wsUrl.Contains("://")) {
                        repository.m_webserviceUrl = repositoryXml.Attributes["webserviceurl"].Value;
                    } else {
                        repository.m_webserviceUrl = repository.m_repositoryUrl.Trim('/') + "/" + wsUrl.Trim('/');
                    }
                }

                repositories.Add(repository);
            }

            return repositories;
        }

        public static Repository getByGuid(string repositoryGuid) {
            Repository repository = new Repository();

            XmlNode repositoryXml = m_repositories.SelectSingleNode("./repository [@guid = '" + repositoryGuid + "']");
            if (repositoryXml != null) {
                repository.m_guid = repositoryXml.Attributes["guid"].Value;
                repository.m_name = repositoryXml.Attributes["name"].Value;

                if (repositoryXml.Attributes["repositoryurl"] != null) {
                    repository.m_repositoryUrl = repositoryXml.Attributes["repositoryurl"].Value;
                    repository.m_webserviceUrl = repository.m_repositoryUrl.Trim('/') + "/" + repository.m_webserviceUrl.Trim('/');
                }

                if (repositoryXml.Attributes["webserviceurl"] != null) {
                    string wsUrl = repositoryXml.Attributes["webserviceurl"].Value;

                    if (wsUrl.Contains("://")) {
                        repository.m_webserviceUrl = repositoryXml.Attributes["webserviceurl"].Value;
                    } else {
                        repository.m_webserviceUrl = repository.m_repositoryUrl.Trim('/') + "/" + wsUrl.Trim('/');
                    }
                }
            }

            return repository;
        }

        //shortcut method to download pack from repo and place it on the server...
        public string fetch(string packageGuid) {

            return fetch(packageGuid, string.Empty);

        }

        public bool HasConnection() {
            
            string strServer = this.RepositoryUrl;

            try {

                HttpWebRequest reqFP = (HttpWebRequest)HttpWebRequest.Create(strServer);
                HttpWebResponse rspFP = (HttpWebResponse)reqFP.GetResponse();

                if (HttpStatusCode.OK == rspFP.StatusCode) {

                    // HTTP = 200 - Internet connection available, server online
                    rspFP.Close();

                    return true;

                } else {

                    // Other status - Server or connection not available

                    rspFP.Close();

                    return false;

                }

            } catch (WebException) {

                // Exception - connection not available

                return false;

            }
        }
        
        public string fetch(string packageGuid, string key) {

            byte[] fileByteArray = new byte[0];

            if (key == string.Empty) {
                if (UmbracoSettings.UseLegacyXmlSchema)
                    fileByteArray = this.Webservice.fetchPackage(packageGuid);
                else
                    fileByteArray = this.Webservice.fetchPackageByVersion(packageGuid, Version.Version41);
            } else {
                fileByteArray = this.Webservice.fetchProtectedPackage(packageGuid, key);
            }

            //successfull 
            if (fileByteArray.Length > 0) {

                // Check for package directory
                if (!System.IO.Directory.Exists(IOHelper.MapPath(packager.Settings.PackagerRoot)))
                    System.IO.Directory.CreateDirectory(IOHelper.MapPath(packager.Settings.PackagerRoot));


                System.IO.FileStream fs1 = null;
                fs1 = new FileStream(IOHelper.MapPath(packager.Settings.PackagerRoot + System.IO.Path.DirectorySeparatorChar.ToString() + packageGuid + ".umb"), FileMode.Create);
                fs1.Write(fileByteArray, 0, fileByteArray.Length);
                fs1.Close();
                fs1 = null;

                return "packages\\" + packageGuid + ".umb";

            } else {

                return "";
            }
        }



    }
}
