//using System;
//using System.Data;
//using System.Configuration;
//using System.Web;
//using System.Web.Security;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.WebParts;
//using System.Web.UI.HtmlControls;
//using System.Xml.Serialization;
//using System.Xml;

//namespace umbraco.webservices
//{
//    public class authentication
//    {


//        /// <summary>
//        /// Standart user-validation. All services must perform this
//        /// </summary>
//        /// <param name="username"></param>
//        /// <param name="password"></param>
//        /// <param name="service"></param>
//        /// <remarks>First checks if the webservices are enabled. Then checks if the user is valid 
//        /// (username and password). Finally it checks if the user has access to the specific service
//        /// </remarks>
//        public static void StandartRequestCheck(string username, string password, authentication.EService service)
//        {
//            // We check if services are enabled and user has access
//            if (!umbraco.UmbracoSettings.Webservices.Enabled)
//                throw new Exception("webservices not enabled");

//            // Validating the user
//            GetUser(username, password);

//            // Checking if user can use that specific service
//            if (!authentication.UserHasAccess(username, service))
//                throw new Exception("user has not access to this service");  
//        }
        
//        /// <summary>
//        /// Gets the umbraco-user from username and password
//        /// </summary>
//        /// <param name="username"></param>
//        /// <param name="password"></param>
//        /// <returns></returns>
//        public static umbraco.BusinessLogic.User GetUser(string username, string password)
//        {
//            umbraco.BusinessLogic.User user;
//            try
//            {
//                user = new umbraco.BusinessLogic.User(username, password);

//                if (user == null)
//                    throw new Exception("Incorrect credentials. No user found. Call aborted");
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Incorrect credentials. Call aborted (error: " + ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace + ")");
//            }
//            return user;
//        }


//        /// <summary>
//        /// List of accesible the services
//        /// </summary>
//        public enum EService
//        {
//            DocumentService,
//            FileService,
//            StylesheetService,
//            MemberService,
//            MaintenanceService,
//            TemplateService
//        };

//        /// <summary>
//        /// Checks if a user has access to a specific webservice
//        /// </summary>
//        /// <param name="username">user to check</param>
//        /// <param name="service">the webservice to check for</param>
//        /// <returns>true, if user has access otherwise false</returns>
//        public static bool UserHasAccess(string username, EService service)
//        {
//            switch (service)
//            {
//                case EService.DocumentService:
//                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.documentServiceUsers, username);
//                case EService.FileService:
//                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.fileServiceUsers, username);
//                case EService.StylesheetService:
//                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.stylesheetServiceUsers, username);
//                case EService.MemberService :
//                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.memberServiceUsers, username);
//                case EService.MaintenanceService:
//                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.maintenanceServiceUsers, username);
//                case EService.TemplateService :
//                    return -1 < Array.IndexOf<string>(umbraco.UmbracoSettings.Webservices.templateServiceUsers, username);
//                default:
//                    return false;
//            }
//        }


//    }
//}
