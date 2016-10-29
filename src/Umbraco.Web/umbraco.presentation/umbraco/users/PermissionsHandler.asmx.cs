using System;
using System.ComponentModel;
using System.IO;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using umbraco.BasePages;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Summary description for GroupPermissionsHandler
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class PermissionsHandler
    {
        /// <summary>
        /// Loads the NodePermissions UserControl with the appropriate properties, renders the contents and returns the output html.
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string GetNodePermissions(int groupId, string nodes)
        {
            Authorize();

            var page = new Page();

            var path = SystemDirectories.Umbraco + "/users/NodePermissions.ascx";
            var nodePermissions = (NodePermissions)page.LoadControl(path);

            nodePermissions.UserGroupID = groupId;
            nodePermissions.NodeID = ToIntArray(nodes);
            nodePermissions.ID = "nodePermissions";

            page.Controls.Add(nodePermissions);
            return GetPageResult(page);
        }

        [WebMethod]
        public string SaveNodePermissions(int groupId, string nodes, string permissions, bool replaceChild)
        {
            Authorize();

            var userService = ApplicationContext.Current.Services.UserService;
            var groupPermissions = new UserGroupPermissions(userService.GetUserGroupById(groupId));
            var actions = umbraco.BusinessLogic.Actions.Action.FromString(permissions);
            groupPermissions.SaveNewPermissions(ToIntArray(nodes), actions, replaceChild);

            return GetNodePermissions(groupId, nodes);
        }

        private void Authorize()
        {
            if (!BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                throw new Exception("Client authorization failed. User is not logged in");
            }
        }

        private static string GetPageResult(Page page)
        {
            var sw = new StringWriter();
            HttpContext.Current.Server.Execute(page, sw, false);
            return sw.ToString();
        }

        private int[] ToIntArray(string nodeIds)
        {
            var s_nodes = nodeIds.Split(',');
            var i_nodes = new int[s_nodes.Length];

            for (int i = 0; i < s_nodes.Length; i++)
            {
                i_nodes[i] = int.Parse(s_nodes[i]);
            }

            return i_nodes;
        }
    }
}