using System.Web;
using System.Web.Services;
using System.ComponentModel;
using System.Web.Script.Services;
using System.Web.UI;
using System.IO;
using System.Collections.Generic;
using Umbraco.Core.IO;
using Umbraco.Web.WebServices;
using Umbraco.Web._Legacy.Actions;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Summary description for PermissionsHandler
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class PermissionsHandler : UmbracoAuthorizedWebService
    {

        /// <summary>
        /// Loads the NodePermissions UserControl with the appropriate properties, renders the contents and returns the output html.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        [WebMethod]
        public string GetNodePermissions(int userID, string nodes)
        {
            AuthorizeRequest(true);

            Page page = new Page();

            string path = SystemDirectories.Umbraco + "/users/NodePermissions.ascx";
            NodePermissions nodePermissions = page.LoadControl(path) as NodePermissions;

            nodePermissions.UserID = userID;
            nodePermissions.NodeID = toIntArray(nodes);
            nodePermissions.ID = "nodePermissions";

            page.Controls.Add(nodePermissions);
            StringWriter sw = new StringWriter();
            HttpContext.Current.Server.Execute(page, sw, false);
            return sw.ToString();
        }




        [WebMethod]
        public string SaveNodePermissions(int userID, string nodes, string permissions, bool replaceChild)
        {
            AuthorizeRequest(true);

            UserPermissions uPermissions = new UserPermissions(Services.UserService.GetUserById(userID));
            List<IAction> actions = Action.FromString(permissions);
            uPermissions.SaveNewPermissions(toIntArray(nodes), actions, replaceChild);

            return GetNodePermissions(userID, nodes);
        }


        private int[] toIntArray(string nodeIds) {

            string[] s_nodes = nodeIds.Split(',');
            int[] i_nodes = new int[s_nodes.Length];

            for (int i = 0; i < s_nodes.Length; i++) {
                i_nodes[i] = int.Parse(s_nodes[i]);
            }

            return i_nodes;

        }

    }
}
