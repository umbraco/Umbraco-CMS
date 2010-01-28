using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace umbraco.webservices
{
    /// <summary>
    /// The base-class all webservices should inherit from
    /// </summary>
    /// <remarks>
    /// This class contains all basic methods for authenticating requests. Do not implement these functions yourself.
    /// </remarks>
    public abstract class BaseWebService : System.Web.Services.WebService
    {
        public abstract Services Service
        {
            get;
        }

        /// <summary>
        /// Enum of services available
        /// </summary>
        public enum Services
        {
            DocumentService, 
            FileService,
            StylesheetService,
            MemberService,
            MaintenanceService,
            TemplateService,
            MediaService
        };

        
        /// <summary>
        /// Gets the umbraco-user from username and password
        /// </summary>
        public umbraco.BusinessLogic.User GetUser(string username, string password)
        {
            try
            {
                umbraco.BusinessLogic.User u =
                    new umbraco.BusinessLogic.User(username, password);
                if (u == null || u.Id == -1)
                    throw new ArgumentException("Invalid username/password");

                return u;
            }
            catch
            {
                return null;
            }
        }




        /// <summary>
        /// Standart user-validation. All services must perform this
        /// </summary>
        public void Authenticate(string username, string password)
        {
            if (!WebservicesEnabled()) throw new Exception("Webservices not enabled");
            if (!UserAuthenticates(username, password)) throw new Exception("The user does not authenticate");
            if (!UserHasAccess(username)) throw new Exception("The user (" + username + ") does not have access to this service");
        }  

        [WebMethod]
        public bool WebservicesEnabled()
        {
            return umbraco.UmbracoSettings.Webservices.Enabled;
        }

        [WebMethod]
        public bool UserAuthenticates(string username, string password)
        {
            if (!WebservicesEnabled()) throw new Exception("Webservices not enabled");
            return GetUser(username, password) != null;
        }

        /// <summary>
        /// Checks if a user has access to a specific webservice
        /// </summary>
        [WebMethod]
        public bool UserHasAccess(string username)
        {
            switch (Service)
            {
                case Services.DocumentService:
                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.documentServiceUsers, username);
                case Services.FileService:
                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.fileServiceUsers, username);
                case Services.StylesheetService:
                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.stylesheetServiceUsers, username);
                case Services.MemberService:
                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.memberServiceUsers, username);
                case Services.MaintenanceService:
                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.maintenanceServiceUsers, username);
                case Services.TemplateService:
                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.templateServiceUsers, username);
                case Services.MediaService:
                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.mediaServiceUsers, username);
                default:
                    return false;
            }
        }



        public class FileIO
        {
            /// <summary>
            /// Validates a filename. Must be used when user inputs a filename
            /// </summary>
            public static bool ValidFileName(string fileName)
            {
                // Check if a "levelup" string is included, so they dont move out of the folder 
                // Dont know if its necesary?
                if (fileName.IndexOf("..") > -1)
                    return false;
                return true;
            }

            /// <summary>
            /// Checks if user has access to a specific folder
            /// </summary>
            public static bool FolderAccess(String folderName)
            {
                // Check if the folder is in "fileServiceFolders" 
                if (Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.fileServiceFolders, folderName) > -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Gets the webservers path for a file
            /// </summary>
            public static string GetFilePath(string folderName, string fileName)
            {
                string FullPath = GetFolderPath(folderName) + fileName;
                return FullPath;
            }

            /// <summary>
            /// Gets the webservers path for a folder 
            /// </summary>
            public static string GetFolderPath(string folderName)
            {
                if (string.IsNullOrEmpty(folderName))
                {
                    return AppRoot;
                }
                else
                {
                    return AppRoot + folderName + @"\";
                }
            }

            /// <summary>
            /// Gets the webservers path for the application
            /// </summary>
            public static string AppRoot
            {
                get
                {
                    return System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
                }
            }

        }

    }

}
