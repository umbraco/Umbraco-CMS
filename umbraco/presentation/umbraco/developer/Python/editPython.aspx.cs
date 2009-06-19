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
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Text.RegularExpressions;
using umbraco.cms.presentation.Trees;

namespace umbraco.cms.presentation.developer
{
    public partial class editPython : BasePages.UmbracoEnsuredPage
    {

        protected PlaceHolder buttons;
        protected uicontrols.CodeArea CodeArea1;

        private void Page_Load(object sender, System.EventArgs e)
        {
            pythonError.Text = "";
            UmbracoPanel1.hasMenu = true;

			if (IsPostBack)
			{
				StreamWriter SW;
				string tempFileName = Server.MapPath(GlobalSettings.Path + "/../python/" + System.DateTime.Now.Ticks.ToString() + "_temp.py");

				//SW = File.CreateText(tempFileName);
				SW = new System.IO.StreamWriter(tempFileName, false, new System.Text.UTF8Encoding(true));
				SW.Write(pythonSource.Text);
				SW.Close();
				string errorMessage = "";
				if (!(SkipTesting.Checked))
				{
					try
					{
						umbraco.scripting.python.compileFile(tempFileName);
					}
					catch (Exception errorPython)
					{
						base.speechBubble(speechBubbleIcon.error, ui.Text("errors", "pythonErrorHeader", base.getUser()), ui.Text("errors", "pythonErrorText", base.getUser()));
						errorHolder.Visible = true;
						closeErrorMessage.Visible = true;
						errorHolder.Attributes.Add("style", "height: 250px; overflow: auto; border: 1px solid CCC; padding: 5px;");
						errorMessage = errorPython.ToString();
						pythonError.Text = errorMessage.Replace("\n", "<br/>\n");
						closeErrorMessage.Visible = true;
					}
				}
				if (errorMessage == "")
				{
					//SW = File.CreateText(Server.MapPath(GlobalSettings.Path + "/../python/" + pythonFileName.Text));
					SW = new System.IO.StreamWriter(Server.MapPath(GlobalSettings.Path + "/../python/" + pythonFileName.Text), false, new System.Text.UTF8Encoding(true));
					SW.Write(pythonSource.Text);
					SW.Close();
					base.speechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "pythonSavedHeader", base.getUser()), ui.Text("speechBubbles", "pythonSavedText", base.getUser()));
				}
				System.IO.File.Delete(tempFileName);
			}
			else
			{
				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadPython>().Tree.Alias)
					.SyncTree(Request.QueryString["file"], false);
			}
        }

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            ImageButton save = UmbracoPanel1.Menu.NewImageButton();
            save.ImageUrl = GlobalSettings.Path + "/images/editor/save.gif";


            // Add source and filename
            String file = Request.QueryString["file"];
            pythonFileName.Text = file;

            StreamReader SR;
            string S;
            SR = File.OpenText(Server.MapPath(GlobalSettings.Path + "/../python/" + file));
            S = SR.ReadToEnd();
            SR.Close();
            pythonSource.Text = S;
        }

        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.Page_Load);
        }
    }
}
