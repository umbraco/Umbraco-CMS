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
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.property;

namespace umbraco.presentation.dialogs
{
	/// <summary>
	/// Summary description for rollBack.
	/// </summary>
	public partial class rollBack : UmbracoEnsuredPage
	{
        private Document currentDoc = new Document(int.Parse(helper.Request("nodeId")));
        
        protected void version_load(object sender, EventArgs e) {

            if (allVersions.SelectedValue != "") {
                diffPanel.Visible = true;
                Document rollback = new Document(currentDoc.Id, new Guid(allVersions.SelectedValue));
                
                propertiesCompare.Text = "<tr><th style='width: 25%;' valign='top'>" + ui.Text("general", "name") + ":</th><td>" + rollback.Text + "</td></tr>";
                propertiesCompare.Text += "<tr><th style='width: 25%;' valign='top'>" + ui.Text("content", "createDate") + ":</th><td>" + rollback.VersionDate.ToLongDateString() + " " + rollback.VersionDate.ToLongTimeString() + ui.Text("general", "by") + ": " + rollback.User.Name + "</td></tr>";

                if (rbl_mode.SelectedValue == "diff")
                    lt_notice.Text = ui.Text("rollback", "diffHelp");
                else
                    lt_notice.Text = ui.Text("rollback", "htmlHelp");
                            

                foreach (Property p in rollback.getProperties) {
                    try {

                        if (p.Value != null) {

                            //new property value... 
                            string thevalue = p.Value.ToString();

                            if (rbl_mode.SelectedValue == "diff") {
                                
                                //if display mode is set to diff...
                                thevalue = library.StripHtml(p.Value.ToString());
                                Property cP = currentDoc.getProperty(p.PropertyType);
                                if (cP != null && cP.Value != null) {

                                    string cThevalue = library.StripHtml(cP.Value.ToString());

                                    propertiesCompare.Text += "<tr><th style='width: 25%;' valign='top'>" + p.PropertyType.Name + ":</th><td>" + library.ReplaceLineBreaks(cms.businesslogic.utilities.Diff.Diff2Html(cThevalue, thevalue)) + "</td></tr>";

                                    
                                } else {
                                    //If no current version of the value... display with no diff.
                                    propertiesCompare.Text += "<tr><th style='width: 25%;' valign='top'>" + p.PropertyType.Name + ":</th><td>" + thevalue + "</td></tr>";
                                }

                            
                            } else {
                                //If display mode is html
                                propertiesCompare.Text += "<tr><th style='width: 25%;' valign='top'>" + p.PropertyType.Name + ":</th><td>" + thevalue + "</td></tr>";
                            }
                            
                        //previewVersionContent.Controls.Add(new LiteralControl("<div style=\"margin-top: 4px; border: 1px solid #DEDEDE; padding: 4px;\"><p style=\"padding: 0px; margin: 0px;\" class=\"guiDialogNormal\"><b>" + p.PropertyType.Name + "</b><br/>"));
                        //previewVersionContent.Controls.Add(new LiteralControl(thevalue));
                        
                        }
                        //previewVersionContent.Controls.Add(new LiteralControl("</p></div>"));
                    } catch { }
                }

                doRollback.Enabled = true;
                doRollback.Attributes.Add("onclick", "return confirm('" + ui.Text("areyousure") + "');");
                
            }else
                diffPanel.Visible = false;

        }

		protected void Page_Load(object sender, System.EventArgs e)
		{

            if (String.IsNullOrEmpty(allVersions.SelectedValue))
                rbl_mode.AutoPostBack = false;
            else
                rbl_mode.AutoPostBack = true;

            currentVersionTitle.Text = currentDoc.Text;
            currentVersionMeta.Text = ui.Text("content", "createDate") + ": " + currentDoc.VersionDate.ToShortDateString() + " " + currentDoc.VersionDate.ToShortTimeString();

            if (!IsPostBack) {
                allVersions.Items.Add(new ListItem(ui.Text("rollback", "selectVersion")+ "...", ""));
                foreach (DocumentVersionList dl in currentDoc.GetVersions()) {
                    allVersions.Items.Add(new ListItem(dl.Text + " (" + ui.Text("content", "createDate") + ": " + dl.Date.ToShortDateString() + " " + dl.Date.ToShortTimeString() + ")", dl.Version.ToString()));
                }
                doRollback.Text = ui.Text("actions", "rollback");
            }

            /*
            foreach(Property p in d.getProperties) 
			{
				string thevalue = p.Value.ToString();
				if (CheckBoxHtml.Checked)
					thevalue = Server.HtmlEncode(thevalue);
				currentVersionContent.Controls.Add(new LiteralControl("<div style=\"margin-top: 4px; border: 1px solid #DEDEDE; padding: 4px;\"><p style=\"padding: 0px; margin: 0px;\" class=\"guiDialogNormal\"><b>" + p.PropertyType.Name + "</b><br/>" + thevalue + "</p></div>"));
			}

			if (allVersions.SelectedValue != "") 
			{
				Document rollback = new Document(d.Id, new Guid(allVersions.SelectedValue));
				previewVersionTitle.Text = rollback.Text;
				previewVersionDetails.Text = "Created at: " + rollback.VersionDate.ToLongDateString() + " " + rollback.VersionDate.ToLongTimeString() + " by: " + rollback.User.Name;
				foreach(Property p in rollback.getProperties) 
				{
					try 
					{
						previewVersionContent.Controls.Add(new LiteralControl("<div style=\"margin-top: 4px; border: 1px solid #DEDEDE; padding: 4px;\"><p style=\"padding: 0px; margin: 0px;\" class=\"guiDialogNormal\"><b>" + p.PropertyType.Name + "</b><br/>"));
						if (p.Value != null) 
						{
							string thevalue = p.Value.ToString();
							if (CheckBoxHtml.Checked)
								thevalue = Server.HtmlEncode(thevalue);
							previewVersionContent.Controls.Add(new LiteralControl(thevalue));
						}
						previewVersionContent.Controls.Add(new LiteralControl("</p></div>"));
					} 
					catch {}
				}
				doRollback.Enabled = true;
				doRollback.Attributes.Add("onClick", "return confirm('" + ui.Text("areyousure") + "');");
			} 
			else 
			{
				doRollback.Enabled = false;
				previewVersionTitle.Text = "No version selected...";
			}

			
		    */									  
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
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

		protected void doRollback_Click(object sender, System.EventArgs e)
		{
            if (allVersions.SelectedValue.Trim() != "") {
                Document d = new Document(int.Parse(helper.Request("nodeId")));
                d.RollBack(new Guid(allVersions.SelectedValue), base.getUser());
                
                BusinessLogic.Log.Add(BusinessLogic.LogTypes.RollBack, base.getUser(), d.Id, "Version rolled back to revision '" + allVersions.SelectedValue + "'");
                
                Document rollback = new Document(d.Id, new Guid(allVersions.SelectedValue));
                feedBackMsg.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
                string[] vars = {rollback.Text, rollback.VersionDate.ToLongDateString()};
                feedBackMsg.Text = ui.Text("rollback", "documentRolledBack", vars, new global::umbraco.BusinessLogic.User(0)) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow + "'>" + ui.Text("closeThisWindow") + "</a>";
                diffPanel.Height = new Unit(200, UnitType.Pixel);             

                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "RollBack", "<script type=\"text/javascript\">\n" + ClientTools.Scripts.ChangeContentFrameUrl("../editContent.aspx?Id=" + d.Id.ToString()) + "\n</script>\n");
            }
		}
	}
}
