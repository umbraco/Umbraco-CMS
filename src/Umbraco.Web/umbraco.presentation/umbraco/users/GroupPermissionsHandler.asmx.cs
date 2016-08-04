using System.ComponentModel;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
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
    public class GroupPermissionsHandler : PermissionsHandlerBase
    {
        /// <summary>
        /// Loads the GroupNodePermissions UserControl with the appropriate properties, renders the contents and returns the output html.
        /// </summary>
        /// <returns></returns>
        [WebMethod]        
        public string GetNodePermissions(int groupId, string nodes)
        {
            Authorize();

            var page = new Page();

            var path = SystemDirectories.Umbraco + "/users/GroupNodePermissions.ascx";
            var nodePermissions = (GroupNodePermissions)page.LoadControl(path);

            nodePermissions.GroupID = groupId;
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
    }
}
