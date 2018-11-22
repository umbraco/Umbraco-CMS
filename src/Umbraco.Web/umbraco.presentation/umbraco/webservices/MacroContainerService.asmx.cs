using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using Umbraco.Web.WebServices;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for MacroContainerService
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/webservices")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class MacroContainerService : UmbracoAuthorizedWebService
    {

        [WebMethod(EnableSession = true)]
        [ScriptMethod]
        public void SetSortOrder(string id, string sortorder)
        {
            if (AuthorizeRequest())
            {
                HttpContext.Current.Session[id + "sortorder"] = sortorder;    
            }
        }
    }
}
