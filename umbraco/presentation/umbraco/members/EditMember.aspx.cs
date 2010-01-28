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

using umbraco.cms.businesslogic.member;
using System.Web.Security;
using umbraco.IO;

namespace umbraco.cms.presentation.members
{
	/// <summary>
	/// Summary description for EditMember.
	/// </summary>
	public partial class EditMember : BasePages.UmbracoEnsuredPage
	{
		protected uicontrols.TabView TabView1;
		protected System.Web.UI.WebControls.TextBox documentName;
		private cms.businesslogic.member.Member _document;
        private MembershipUser m_Member;
		controls.ContentControl tmp;
        protected umbraco.uicontrols.UmbracoPanel m_MemberShipPanel = new umbraco.uicontrols.UmbracoPanel(); 

		protected TextBox MemberLoginNameTxt = new TextBox();
		protected PlaceHolder MemberPasswordTxt = new PlaceHolder();
		protected TextBox MemberEmail = new TextBox();
		protected controls.DualSelectbox _memberGroups = new controls.DualSelectbox();


		protected void Page_Load(object sender, System.EventArgs e)
		{

            // Add password changer
            MemberPasswordTxt.Controls.Add(new UserControl().LoadControl(SystemDirectories.Umbraco + "/controls/passwordChanger.ascx"));

            if (Member.InUmbracoMemberMode())
            {
                _document = new cms.businesslogic.member.Member(int.Parse(Request.QueryString["id"]));
                m_Member = Membership.GetUser(_document.Id);
                tmp = new controls.ContentControl(_document, controls.ContentControl.publishModes.NoPublish, "TabView1");
                tmp.Width = Unit.Pixel(666);
                tmp.Height = Unit.Pixel(666);
                plc.Controls.Add(tmp);

                if (!IsPostBack)
                {
                    MemberLoginNameTxt.Text = _document.LoginName;
                    MemberEmail.Text = _document.Email;
                }
                tmp.PropertiesPane.addProperty(ui.Text("login"), MemberLoginNameTxt);
                tmp.PropertiesPane.addProperty(ui.Text("password"), MemberPasswordTxt);
                tmp.PropertiesPane.addProperty("Email", MemberEmail);
            }
            else
            {
                m_Member = Membership.GetUser(Request.QueryString["id"]);
                MemberLoginNameTxt.Text = m_Member.UserName;
                if (!IsPostBack)
                {
                    MemberEmail.Text = m_Member.Email;
                }

                m_MemberShipPanel.Width = 300;
                m_MemberShipPanel.Text = ui.Text("edit") + " " + m_Member.UserName;
                umbraco.uicontrols.Pane props = new umbraco.uicontrols.Pane();
                MemberLoginNameTxt.Enabled = false;

                // check for pw support
                if (!Membership.Provider.EnablePasswordRetrieval) {
                    MemberPasswordTxt.Controls.Clear();
                    MemberPasswordTxt.Controls.Add(
                        new LiteralControl("<em>" + ui.Text("errorHandling", "errorChangingProviderPassword") + "</em>"));
                }

                props.Controls.Add(AddProperty(ui.Text("login"), MemberLoginNameTxt));
                props.Controls.Add(AddProperty(ui.Text("password"), MemberPasswordTxt));
                props.Controls.Add(AddProperty("Email", MemberEmail));
                m_MemberShipPanel.Controls.Add(props);
                plc.Controls.Add(m_MemberShipPanel);
            }

			// Groups
			umbraco.uicontrols.Pane p = new umbraco.uicontrols.Pane();
			_memberGroups.ID = "Membergroups";
			_memberGroups.Width = 175;
			string selectedMembers = "";
			foreach(string role in Roles.GetAllRoles()) 
			{
                // if a role starts with __umbracoRole we won't show it as it's an internal role used for public access
                if (!role.StartsWith("__umbracoRole"))
                {
                    ListItem li = new ListItem(role);
                    if (!IsPostBack)
                    {

                        if (Roles.IsUserInRole(m_Member.UserName, role))
                            selectedMembers += role + ",";
                    }
                    _memberGroups.Items.Add(li);
                }
			}
			_memberGroups.Value = selectedMembers;

            p.addProperty(ui.Text("membergroup"), _memberGroups);

            if (Member.InUmbracoMemberMode())
            {
                tmp.tpProp.Controls.Add(p);
                tmp.Save += new System.EventHandler(tmp_save);
            }
            else
                m_MemberShipPanel.Controls.Add(p);

		}

        void menuSave_Click(object sender, ImageClickEventArgs e)
        {
            tmp_save(sender, e);
        }
		protected void tmp_save(object sender, System.EventArgs e) {
            if (Member.InUmbracoMemberMode())
            {
                _document.LoginName = MemberLoginNameTxt.Text;
                _document.Email = MemberEmail.Text;

                // Check if password should be changed
                string tempPassword = ((controls.passwordChanger)MemberPasswordTxt.Controls[0]).Password;
                if (tempPassword.Trim() != "")
                    _document.Password = tempPassword;

                // Groups
                foreach (ListItem li in _memberGroups.Items)
                    if (("," + _memberGroups.Value + ",").IndexOf("," + li.Value + ",") > -1)
                    {
                        if (!Roles.IsUserInRole(_document.LoginName, li.Value))
                            Roles.AddUserToRole(_document.LoginName, li.Value);
                    }
                    else if (Roles.IsUserInRole(_document.LoginName, li.Value))
                    {
                        Roles.RemoveUserFromRole(_document.LoginName, li.Value);
                    }
                // refresh cache
                _document.XmlGenerate(new System.Xml.XmlDocument());
                _document.Save();
            }
            else {
                m_Member.Email = MemberEmail.Text;
                if (Membership.Provider.EnablePasswordRetrieval)
                {
                    string tempPassword = ((controls.passwordChanger)MemberPasswordTxt.Controls[0]).Password;
                    if (tempPassword.Trim() != "")
                        m_Member.ChangePassword(m_Member.GetPassword(), tempPassword);
                }
                Membership.UpdateUser(m_Member);
                // Groups
                foreach (ListItem li in _memberGroups.Items)
                    if (("," + _memberGroups.Value + ",").IndexOf("," + li.Value + ",") > -1)
                    {
                        if (!Roles.IsUserInRole(m_Member.UserName, li.Value))
                            Roles.AddUserToRole(m_Member.UserName, li.Value);
                    }
                    else if (Roles.IsUserInRole(m_Member.UserName, li.Value))
                    {
                        Roles.RemoveUserFromRole(m_Member.UserName, li.Value);
                    }

            }

			this.speechBubble(BasePages.BasePage.speechBubbleIcon.save,ui.Text("speechBubbles", "editMemberSaved", base.getUser()),"");
		}

        private umbraco.uicontrols.PropertyPanel AddProperty(string Caption, Control C) {
            umbraco.uicontrols.PropertyPanel pp = new umbraco.uicontrols.PropertyPanel();
            pp.Controls.Add(C);
            pp.Text = Caption;
            return pp;
        }
		
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//

            if (!Member.InUmbracoMemberMode()) {
                m_MemberShipPanel.hasMenu = true;
                umbraco.uicontrols.MenuImageButton menuSave = m_MemberShipPanel.Menu.NewImageButton();
                menuSave.ID = m_MemberShipPanel.ID + "_save";
                menuSave.ImageUrl = SystemDirectories.Umbraco + "/images/editor/save.gif";
                menuSave.Click += new ImageClickEventHandler(menuSave_Click);
                menuSave.AltText = ui.Text("buttons", "save", null);
            
            }
            InitializeComponent();
            base.OnInit(e);
        }
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion
	}
}
