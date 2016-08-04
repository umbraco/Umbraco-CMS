using System.ComponentModel;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using Umbraco.Core.IO;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Summary description for PermissionsHandler
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class PermissionsHandler : PermissionsHandlerBase
    {
        /// <summary>
        /// Loads the NodePermissions UserControl with the appropriate properties, renders the contents and returns the output html.
        /// </summary>
        /// <returns></returns>
        [WebMethod]        
        public string GetNodePermissions(int userId, string nodes)
        {
            Authorize();

            var page = new Page();

            var path = SystemDirectories.Umbraco + "/users/NodePermissions.ascx";
            var nodePermissions = (NodePermissions)page.LoadControl(path);

            nodePermissions.UserID = userId;
            nodePermissions.NodeID = ToIntArray(nodes);
            nodePermissions.ID = "nodePermissions";
            
            page.Controls.Add(nodePermissions);
            return GetPageResult(page);
        }

        [WebMethod]
        public string SaveNodePermissions(int userId, string nodes, string permissions, bool replaceChild)
        {
			Authorize();

            var userPermissions = new UserPermissions(BusinessLogic.User.GetUser(userId));
            var actions = umbraco.BusinessLogic.Actions.Action.FromString(permissions);
            userPermissions.SaveNewPermissions(ToIntArray(nodes), actions, replaceChild);

            return GetNodePermissions(userId, nodes);
        }
    }
}
