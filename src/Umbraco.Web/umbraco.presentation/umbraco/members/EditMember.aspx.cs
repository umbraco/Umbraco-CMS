using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;
using System.Web.Security;

namespace umbraco.cms.presentation.members
{
	/// <summary>
	/// Summary description for EditMember.
	/// </summary>
	public partial class EditMember : BasePages.UmbracoEnsuredPage
	{
	    public EditMember()
	    {
            CurrentApp = BusinessLogic.DefaultApps.member.ToString();
        }
		protected uicontrols.TabView TabView1;
		protected TextBox documentName;
		private Member _document;
        private MembershipUser _member;
		controls.ContentControl _contentControl;
        protected uicontrols.UmbracoPanel m_MemberShipPanel = new uicontrols.UmbracoPanel(); 

		protected TextBox MemberLoginNameTxt = new TextBox();
	    protected RequiredFieldValidator MemberLoginNameVal = new RequiredFieldValidator();

		protected PlaceHolder MemberPasswordTxt = new PlaceHolder();
		protected TextBox MemberEmail = new TextBox();
		protected controls.DualSelectbox _memberGroups = new controls.DualSelectbox();


		protected void Page_Load(object sender, EventArgs e)
		{

            // Add password changer
            MemberPasswordTxt.Controls.Add(new UserControl().LoadControl(SystemDirectories.Umbraco + "/controls/passwordChanger.ascx"));

            if (Member.InUmbracoMemberMode())
            {
                _document = new Member(int.Parse(Request.QueryString["id"]));
                _member = Membership.GetUser(_document.Id);
                _contentControl = new controls.ContentControl(_document, controls.ContentControl.publishModes.NoPublish, "TabView1");
                _contentControl.Width = Unit.Pixel(666);
                _contentControl.Height = Unit.Pixel(666);

                //this must be set to false as we don't want to proceed to save anything if the page is invalid
                _contentControl.SavePropertyDataWhenInvalid = false;

                plc.Controls.Add(_contentControl);

                if (!IsPostBack)
                {
                    MemberLoginNameTxt.Text = _document.LoginName;
                    MemberEmail.Text = _document.Email;
                }
                var ph = new PlaceHolder();
                MemberLoginNameTxt.ID = "loginname";
                ph.Controls.Add(MemberLoginNameTxt);
                ph.Controls.Add(MemberLoginNameVal);
                MemberLoginNameVal.ControlToValidate = MemberLoginNameTxt.ID;
                string[] errorVars = { ui.Text("login") };
                MemberLoginNameVal.ErrorMessage = " " + ui.Text("errorHandling", "errorMandatoryWithoutTab", errorVars, null);
                MemberLoginNameVal.EnableClientScript = false;
                MemberLoginNameVal.Display = ValidatorDisplay.Dynamic;

                _contentControl.PropertiesPane.addProperty(ui.Text("login"), ph);
                _contentControl.PropertiesPane.addProperty(ui.Text("password"), MemberPasswordTxt);
                _contentControl.PropertiesPane.addProperty("Email", MemberEmail);
            }
            else
            {
                _member = Membership.GetUser(Request.QueryString["id"]);
                MemberLoginNameTxt.Text = _member.UserName;
                if (!IsPostBack)
                {
                    MemberEmail.Text = _member.Email;
                }

                m_MemberShipPanel.Width = 300;
                m_MemberShipPanel.Text = ui.Text("edit") + " " + _member.UserName;
                var props = new uicontrols.Pane();
                MemberLoginNameTxt.Enabled = false;

                // check for pw support
                if (!Membership.Provider.EnablePasswordRetrieval)
                {
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
			var p = new uicontrols.Pane();
			_memberGroups.ID = "Membergroups";
			_memberGroups.Width = 175;
			var selectedMembers = "";
			foreach(var role in Roles.GetAllRoles()) 
			{
                // if a role starts with __umbracoRole we won't show it as it's an internal role used for public access
                if (!role.StartsWith("__umbracoRole"))
                {
                    var li = new ListItem(role);
                    if (!IsPostBack)
                    {

                        if (Roles.IsUserInRole(_member.UserName, role))
                            selectedMembers += role + ",";
                    }
                    _memberGroups.Items.Add(li);
                }
			}
			_memberGroups.Value = selectedMembers;

            p.addProperty(ui.Text("membergroup"), _memberGroups);

            if (Member.InUmbracoMemberMode())
            {
                _contentControl.tpProp.Controls.Add(p);
                _contentControl.Save += new System.EventHandler(tmp_save);
            }
            else
                m_MemberShipPanel.Controls.Add(p);

		}

        void MenuSaveClick(object sender, ImageClickEventArgs e)
        {
           
            tmp_save(sender, e);
            
        }

	    protected void tmp_save(object sender, EventArgs e)
	    {
	        Page.Validate();
	        if (!Page.IsValid)
	        {
                foreach (uicontrols.TabPage tp in _contentControl.GetPanels())
                {
                    tp.ErrorControl.Visible = true;
                    tp.ErrorHeader = ui.Text("errorHandling", "errorHeader");
                    tp.CloseCaption = ui.Text("close");
                }
	        }
	        else
	        {

                if (Page.IsPostBack)
                {
                    // hide validation summaries
                    foreach (uicontrols.TabPage tp in _contentControl.GetPanels())
                    {
                        tp.ErrorControl.Visible = false;
                    }
                }    

	            if (Member.InUmbracoMemberMode())
	            {
	                _document.LoginName = MemberLoginNameTxt.Text;
	                _document.Email = MemberEmail.Text;

	                // Check if password should be changed
	                string tempPassword = ((controls.passwordChanger) MemberPasswordTxt.Controls[0]).Password;
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
	            else
	            {
	                _member.Email = MemberEmail.Text;
	                if (Membership.Provider.EnablePasswordRetrieval)
	                {
	                    string tempPassword = ((controls.passwordChanger) MemberPasswordTxt.Controls[0]).Password;
	                    if (tempPassword.Trim() != "")
	                        _member.ChangePassword(_member.GetPassword(), tempPassword);
	                }
	                Membership.UpdateUser(_member);
	                // Groups
	                foreach (ListItem li in _memberGroups.Items)
	                    if (("," + _memberGroups.Value + ",").IndexOf("," + li.Value + ",") > -1)
	                    {
	                        if (!Roles.IsUserInRole(_member.UserName, li.Value))
	                            Roles.AddUserToRole(_member.UserName, li.Value);
	                    }
	                    else if (Roles.IsUserInRole(_member.UserName, li.Value))
	                    {
	                        Roles.RemoveUserFromRole(_member.UserName, li.Value);
	                    }

	            }

	            this.speechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editMemberSaved", base.getUser()), "");
	        }
	    }

	    private uicontrols.PropertyPanel AddProperty(string caption, Control c)
	    {
	        var pp = new uicontrols.PropertyPanel();
	        pp.Controls.Add(c);
	        pp.Text = caption;
	        return pp;
	    }

	    override protected void OnInit(EventArgs e)
		{
		    if (!Member.InUmbracoMemberMode())
		    {
		        m_MemberShipPanel.hasMenu = true;
		        umbraco.uicontrols.MenuImageButton menuSave = m_MemberShipPanel.Menu.NewImageButton();
		        menuSave.ID = m_MemberShipPanel.ID + "_save";
		        menuSave.ImageUrl = SystemDirectories.Umbraco + "/images/editor/save.gif";
		        menuSave.Click += new ImageClickEventHandler(MenuSaveClick);
		        menuSave.AltText = ui.Text("buttons", "save", null);
		    }
		    base.OnInit(e);
        }
		
	}
}
