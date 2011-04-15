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

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);

			node = new cms.businesslogic.CMSNode(int.Parse(helper.Request("id")));

			HtmlTable ht = new HtmlTable();
			ht.CellPadding = 4;

			HtmlTableRow captions = new HtmlTableRow();
			captions.Cells.Add(new HtmlTableCell());

            ArrayList actionList = BusinessLogic.Actions.Action.GetAll();
            foreach (interfaces.IAction a in actionList)
            {
				if (a.CanBePermissionAssigned) 
				{
					HtmlTableCell hc = new HtmlTableCell();
					hc.Attributes.Add("class", "guiDialogTinyMark");
					hc.Controls.Add(new LiteralControl(ui.Text("actions", a.Alias)));
					captions.Cells.Add(hc);
				}
			}
			ht.Rows.Add(captions);

			foreach (BusinessLogic.User u in BusinessLogic.User.getAll()) 
			{
				// Not disabled users and not system account
				if (!u.Disabled && u.Id > 0) 
				{
					HtmlTableRow hr = new HtmlTableRow();

					HtmlTableCell hc = new HtmlTableCell();
					hc.Attributes.Add("class", "guiDialogTinyMark");
					hc.Controls.Add(new LiteralControl(u.Name));
					hr.Cells.Add(hc);

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
							permissions.Add(c);
							hr.Cells.Add(cell);
						}
							
					}
					ht.Rows.Add(hr);
				}
			}
			PlaceHolder1.Controls.Add(ht);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion

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
			pane_form.Visible = false;
            panel_buttons.Visible = false;


		}

	}
}
