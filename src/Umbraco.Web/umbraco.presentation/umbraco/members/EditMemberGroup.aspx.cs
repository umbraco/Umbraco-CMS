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

using System.Web.Security;
using umbraco.cms.businesslogic.member;
using umbraco.cms.presentation.Trees;
using umbraco.uicontrols;

namespace umbraco.presentation.members
{
	/// <summary>
	/// Summary description for EditMemberGroup.
	/// </summary>
	public partial class EditMemberGroup : BasePages.UmbracoEnsuredPage
	{
	    public EditMemberGroup()
	    {
            CurrentApp = BusinessLogic.DefaultApps.member.ToString();

	    }

        private MemberGroup _memberGroup = null;
        protected MenuButton save = null;
        string _memberGroupId = String.Empty;

		protected void Page_Load(object sender, System.EventArgs e)
		{
            _memberGroupId = !String.IsNullOrEmpty(memberGroupName.Value) ? memberGroupName.Value : Request.QueryString["id"];

            // Restore any escaped apostrophe for name look up
            _memberGroupId = _memberGroupId.Replace("\\'", "'");

            if (!IsPostBack)
			{
                ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadMemberGroups>().Tree.Alias)
                    .SyncTree(_memberGroupId, false);
			}

            if (!Member.IsUsingUmbracoRoles())
            {
                NameTxt.Enabled = false;
                save.Enabled = false;
                NameTxt.Text = _memberGroupId + " (not editable from umbraco)";
            }
            else
            {
                _memberGroup = MemberGroup.GetByName(_memberGroupId);

                if (!IsPostBack)
                {
                    NameTxt.Text = _memberGroup.Text;
                }
            }
		}

		private void save_click(object sender, EventArgs e) 
		{
			_memberGroup.Text = NameTxt.Text;
            memberGroupName.Value = NameTxt.Text;
            _memberGroup.Save();
            this.ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editMemberGroupSaved", base.getUser()),"");

            ClientTools
                .RefreshTree(TreeDefinitionCollection.Instance.FindTree<loadMemberGroups>().Tree.Alias);
            
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
			Panel1.hasMenu = true;
			save = Panel1.Menu.NewButton();
            save.Text = ui.Text("save");
            save.Click += new EventHandler(save_click);
            save.ButtonType = MenuButtonType.Primary;

			Panel1.Text = ui.Text("membergroup");
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
