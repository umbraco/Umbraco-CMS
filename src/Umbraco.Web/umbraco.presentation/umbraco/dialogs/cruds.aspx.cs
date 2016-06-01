using Umbraco.Core.Services;
using System;
using System.Linq;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Web;
using umbraco.cms.businesslogic;
using Umbraco.Web.UI.Pages;
using Umbraco.Web._Legacy.Actions;

namespace umbraco.dialogs
{
    /// <summary>
    /// Summary description for cruds.
    /// </summary>
    public partial class cruds : UmbracoEnsuredPage
    {

        public cruds()
        {
            CurrentApp = Constants.Applications.Content.ToString();

        }

        private readonly Dictionary<string, HtmlTableRow> _permissions = new Dictionary<string, HtmlTableRow>();
        private CMSNode _node;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            Button1.Text = Services.TextService.Localize("update");
            pane_form.Text = "Set permissions for the page " + _node.Text;
        }

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _node = new CMSNode(Request.GetItemAs<int>("id"));

            var ht = new HtmlTable();
            ht.Attributes.Add("class", "table");

            var names = new HtmlTableRow();

            var corner = new HtmlTableCell("th");
            corner.Style.Add("border", "none");
            names.Cells.Add(corner);
            
            foreach (var a in ActionsResolver.Current.Actions)
            {
                if (a.CanBePermissionAssigned == false) continue;

                var permissionRow = new HtmlTableRow();
                var label = new HtmlTableCell
                    {
                        InnerText = Services.TextService.Localize("actions", a.Alias)
                    };
                permissionRow.Cells.Add(label);
                _permissions.Add(a.Alias, permissionRow);
            }

            ht.Rows.Add(names);

            long totalUsers;
            foreach (var u in Services.UserService.GetAll(0, int.MaxValue, out totalUsers))
            {
                // Not disabled users and not system account
                if (u.IsApproved && u.Id > 0)
                {
                    var hc = new HtmlTableCell("th")
                        {
                            InnerText = u.Name
                        };
                    hc.Style.Add("text-align", "center");
                    hc.Style.Add("border", "none");
                    names.Cells.Add(hc);

                    foreach (var a in ActionsResolver.Current.Actions)
                    {
                        var chk = new CheckBox
                            {
                                //Each checkbox is named with the user _ permission alias so we can parse
                                ID = u.Id + "_" + a.Letter                                
                            };

                        if (a.CanBePermissionAssigned == false) continue;

                        var permission = Services.UserService.GetPermissions(u, _node.Path);

                        if (permission.AssignedPermissions.Contains(a.Letter.ToString(), StringComparer.Ordinal))
                        {
                            chk.Checked = true;
                        }

                        var cell = new HtmlTableCell();
                        cell.Style.Add("text-align", "center");
                        cell.Controls.Add(chk);

                        _permissions[a.Alias].Cells.Add(cell);
                    }
                }
            }

            //add all collected rows
            foreach (var perm in _permissions.Values)
            {
                ht.Rows.Add(perm);    
            }

            PlaceHolder1.Controls.Add(ht);
        }


        protected void Button1_Click(object sender, EventArgs e)
        {
            //get non disabled, non admin users and project to a dictionary, 
            // the string (value) portion will store the array of chars = their permissions
            long totalUsers;
            var usersPermissions = Services.UserService.GetAll(0, int.MaxValue, out totalUsers)
                .Where(user => user.IsApproved && user.Id > 0)
                .ToDictionary(user => user, user => "");
            
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
                        //get the reference to the user
                        var user = usersPermissions.Keys.Single(x => x.Id == int.Parse(split[0]));
                        //get the char permission
                        var permAlias = split[1];
                        //now append that char permission to the user
                        usersPermissions[user] += permAlias;    
                    }
                }
            }
            
            // Loop through the users and update their permissions
            foreach (var user in usersPermissions)
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
            //FeedBackMessage.Text = "<div class=\"feedbackCreate\">" + Services.TextService.Localize("rights") + " " + Services.TextService.Localize("ok") + "</div>";
            feedback1.type = uicontrols.Feedback.feedbacktype.success;
            feedback1.Text = Services.TextService.Localize("rights") + " " + Services.TextService.Localize("ok");
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
