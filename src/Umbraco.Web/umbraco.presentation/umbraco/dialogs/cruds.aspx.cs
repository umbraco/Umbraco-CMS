using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Web;
using umbraco.cms.businesslogic;

namespace umbraco.dialogs
{
    /// <summary>
    /// Summary description for cruds.
    /// </summary>
    [Obsolete("Remove this for 7.7 release!")]
    public class cruds : BasePages.UmbracoEnsuredPage
    {

        public cruds()
        {
            CurrentApp = BusinessLogic.DefaultApps.content.ToString();

        }

        private readonly Dictionary<string, HtmlTableRow> _permissions = new Dictionary<string, HtmlTableRow>();
        private CMSNode _node;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            Button1.Text = ui.Text("update");
            pane_form.Text = ui.Text("actions", "SetPermissionsForThePage",_node.Text); 
        }

        protected override void OnInit(EventArgs e)
        {
            throw new NotSupportedException("This cruds.aspx.cs needs to be removed, it is no longer required");
        }


        protected void Button1_Click(object sender, EventArgs e)
        {
            //get user groups and project to a dictionary, 
            // the string (value) portion will store the array of chars = their permissions
            var userService = ApplicationContext.Current.Services.UserService;
            var groupsPermissions = userService.GetAllUserGroups()
                .ToDictionary(group => group, group => string.Empty);
            
            //iterate over each row which equals:
            // * a certain permission and the user's who will be allowed/denied that permission
            foreach (var row in _permissions)
            {
                //iterate each cell that is not the first cell (which is the permission header cell)
                for (var i = 1; i < row.Value.Cells.Count; i++)
                {
                    var currCell = row.Value.Cells[i];
                    //there's only one control per cell = the check box
                    var chk = (CheckBox)currCell.Controls[0];
                    //if it's checked then append the permissions
                    if (chk.Checked)
                    {
                        //now we will parse the checkbox ID which is the userId_permissionAlias
                        var split = chk.ID.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                        //get the reference to the group
                        var group = groupsPermissions.Keys.Single(x => x.Id == int.Parse(split[0]));
                        //get the char permission
                        var permAlias = split[1];
                        //now append that char permission to the user
                        groupsPermissions[group] += permAlias;    
                    }
                }
            }
            
            // Loop through the users and update their permissions
            foreach (var user in groupsPermissions)
            {
                //default to "-" for whatever reason (was here before so we'll leave it)
                var cruds = "-";
                if (user.Value.IsNullOrWhiteSpace() == false)
                {
                    cruds = user.Value;
                }
                BusinessLogic.Permission.UpdateCruds(user.Key, _node, cruds);       
            }

            // Update feedback message
            //FeedBackMessage.Text = "<div class=\"feedbackCreate\">" + ui.Text("rights") + " " + ui.Text("ok") + "</div>";
            feedback1.type = uicontrols.Feedback.feedbacktype.success;
            feedback1.Text = ui.Text("rights") + " " + ui.Text("ok");
            PlaceHolder1.Visible = false;
            panel_buttons.Visible = false;
        }

        /// <summary>
        /// pane_form control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_form;

        /// <summary>
        /// feedback1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Feedback feedback1;

        /// <summary>
        /// PlaceHolder1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder PlaceHolder1;

        /// <summary>
        /// panel_buttons control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl panel_buttons;

        /// <summary>
        /// Button1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button Button1;

    }
}
