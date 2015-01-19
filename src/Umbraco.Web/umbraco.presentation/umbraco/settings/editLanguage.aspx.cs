using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using Umbraco.Web.Trees;

namespace umbraco.settings
{
	/// <summary>
	/// Summary description for editLanguage.
	/// </summary>
    [WebformsPageTreeAuthorize(Constants.Trees.Languages)]
	public partial class editLanguage : BasePages.UmbracoEnsuredPage
	{
	    public editLanguage()
	    {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();

	    }
		protected System.Web.UI.WebControls.TextBox NameTxt;
		protected System.Web.UI.WebControls.Literal DisplayName;
		cms.businesslogic.language.Language currentLanguage;
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			currentLanguage = new cms.businesslogic.language.Language(int.Parse(helper.Request("id")));
           

			// Put user code to initialize the page here
            Panel1.Text = ui.Text("editlanguage");
            pp_language.Text = ui.Text("language", "displayName", base.getUser());
            if (!IsPostBack) 
			{
				updateCultureList();

				ClientTools
					.SetActiveTreeType(Constants.Trees.Languages)
					.SyncTree(helper.Request("id"), false);
			}
			
		}

		private void updateCultureList() 
		{
            SortedList sortedCultures = new SortedList();
            Cultures.Items.Clear();
            Cultures.Items.Add(new ListItem(ui.Text("choose") + "...", ""));
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
                sortedCultures.Add(ci.DisplayName + "|||" + Guid.NewGuid().ToString(), ci.Name);

            IDictionaryEnumerator ide = sortedCultures.GetEnumerator();
            while (ide.MoveNext())
            {
                ListItem li = new ListItem(ide.Key.ToString().Substring(0, ide.Key.ToString().IndexOf("|||")), ide.Value.ToString());
                if (ide.Value.ToString() == currentLanguage.CultureAlias)
                    li.Selected = true;

                Cultures.Items.Add(li);
            }
        }

		private void save_click(object sender, EventArgs e) 
		{
			currentLanguage.CultureAlias = Cultures.SelectedValue;
		    currentLanguage.Save();
			updateCultureList();

            ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "languageSaved"), "");	
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			Panel1.hasMenu = true;
			var save = Panel1.Menu.NewButton();
			save.Click += save_click;
			save.ToolTip = ui.Text("save");
            save.Text = ui.Text("save");
		    save.ID = "save";
            save.ButtonType = uicontrols.MenuButtonType.Primary;
	
			Panel1.Text = ui.Text("language", "editLanguage");

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
