using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for cruds.
	/// </summary>
	public partial class cruds : BasePages.UmbracoEnsuredPage
	{

	    public cruds()
	    {
            CurrentApp = BusinessLogic.DefaultApps.content.ToString();

	    }

		private ArrayList permissions = new ArrayList();
		private cms.businesslogic.CMSNode node;


		protected void Page_Load(object sender, System.EventArgs e)
		{
			Button1.Text = ui.Text("update");
            pane_form.Text = "Set permissions for the page " + node.Text;
			
			// Put user code to initialize the page here
		}

		override protected void OnInit(EventArgs e)
		{
			base.OnInit(e);

			node = new cms.businesslogic.CMSNode(int.Parse(helper.Request("id")));

            HtmlTable ht = new HtmlTable();
            ht.Attributes.Add("class", "table");

        	HtmlTableRow names = new HtmlTableRow();

            var corner = new HtmlTableCell("th");
            corner.Style.Add("border", "none");
			names.Cells.Add(corner);


            ArrayList actionList = BusinessLogic.Actions.Action.GetAll();
            Dictionary<string, HtmlTableRow> permissions = new Dictionary<string, HtmlTableRow>();


            foreach (interfaces.IAction a in actionList)
            {
				if (a.CanBePermissionAssigned) 
				{

                    HtmlTableRow permission = new HtmlTableRow();
                    HtmlTableCell label = new HtmlTableCell();
                    label.InnerText = ui.Text("actions", a.Alias);
                    permission.Cells.Add(label);
                    permissions.Add(a.Alias, permission);
				}
			}
			
            ht.Rows.Add(names);


			foreach (BusinessLogic.User u in BusinessLogic.User.getAll()) 
			{
				// Not disabled users and not system account
				if (!u.Disabled && u.Id > 0) 
				{
					HtmlTableCell hc = new HtmlTableCell("th");
                    hc.InnerText = u.Name;
                    hc.Style.Add("text-align", "center");
                    hc.Style.Add("border", "none");
					names.Cells.Add(hc);

					foreach (interfaces.IAction a in BusinessLogic.Actions.Action.GetAll()) 
					{
						CheckBox c = new CheckBox();
						c.ID = u.Id + "_" + a.Letter;
						if (a.CanBePermissionAssigned) 
						{
							if (u.GetPermissions(node.Path).IndexOf(a.Letter) > -1)
								c.Checked = true;
							
                            HtmlTableCell cell = new HtmlTableCell();
							cell.Style.Add("text-align", "center");
							cell.Controls.Add(c);

                            permissions[a.Alias].Cells.Add(cell);
						}
							
					}
				}
			}

            //add all collected rows
            foreach (var perm in permissions.Values)
                ht.Rows.Add(perm);

			PlaceHolder1.Controls.Add(ht);
		}
		

		protected void Button1_Click(object sender, System.EventArgs e)
		{
			// First off - load all users
			Hashtable allUsers = new Hashtable();
			foreach (BusinessLogic.User u in BusinessLogic.User.getAll()) 
				if (!u.Disabled && u.Id > 0)
					allUsers.Add(u.Id, "");

			foreach (CheckBox c in permissions) 
			{
				// Update the user with the new permission
				if (c.Checked)
					allUsers[int.Parse(c.ID.Substring(0,c.ID.IndexOf("_")))] = (string) allUsers[int.Parse(c.ID.Substring(0,c.ID.IndexOf("_")))] + c.ID.Substring(c.ID.IndexOf("_")+1, c.ID.Length-c.ID.IndexOf("_")-1);
			}


			// Loop through the users and update their Cruds...
			IDictionaryEnumerator uEnum = allUsers.GetEnumerator();
			while (uEnum.MoveNext()) 
			{
				string cruds = "-";
				if (uEnum.Value.ToString().Trim() != "")
					cruds = uEnum.Value.ToString();

				BusinessLogic.Permission.UpdateCruds(BusinessLogic.User.GetUser(int.Parse(uEnum.Key.ToString())), node, cruds);

				BusinessLogic.User.GetUser(int.Parse(uEnum.Key.ToString())).initCruds();
			}

			// Update feedback message
			//FeedBackMessage.Text = "<div class=\"feedbackCreate\">" + ui.Text("rights") + " " + ui.Text("ok") + "</div>";
            feedback1.type = umbraco.uicontrols.Feedback.feedbacktype.success;
            feedback1.Text = ui.Text("rights") + " " + ui.Text("ok");
			PlaceHolder1.Visible = false;
            panel_buttons.Visible = false;


		}

	}
}
