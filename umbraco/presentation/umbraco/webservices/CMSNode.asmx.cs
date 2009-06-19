using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;

using umbraco.cms.businesslogic;
using umbraco.BusinessLogic;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for CMSNode
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/webservices/")]
    [ScriptService]
    public class CMSNode : System.Web.Services.WebService
    {

        [WebMethod]
        public string GetNodeName(string ContextID, int NodeId)
        {
            if (BasePages.BasePage.ValidateUserContextID(ContextID))
                return getNodeName(NodeId);

            return "";
        }

        private string getNodeName(int NodeId)
        {
            cms.businesslogic.CMSNode n = new cms.businesslogic.CMSNode(NodeId);
            return n.Text;
        }
    }
}
