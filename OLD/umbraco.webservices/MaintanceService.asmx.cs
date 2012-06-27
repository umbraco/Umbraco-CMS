using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;

namespace umbraco.webservices.maintenance
{

    [WebService(Namespace = "http://umbraco.org/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class maintenanceService : BaseWebService
    {

        override public Services Service
        {
            get
            {
                return Services.MaintenanceService;
            }
        }

        [WebMethod]
        public string getWebservicesVersion(string username, string password)
        {
            // We check if services are enabled and user has access
            Authenticate(username, password);
            
            Version thisVersion = new Version(0, 9);
            return Convert.ToString(thisVersion);
        }

        [WebMethod]
        public void restartApplication(string username, string password)
        {
            // We check if services are enabled and user has access
            Authenticate(username, password);

            System.Web.HttpRuntime.UnloadAppDomain();
        }

    }
}
