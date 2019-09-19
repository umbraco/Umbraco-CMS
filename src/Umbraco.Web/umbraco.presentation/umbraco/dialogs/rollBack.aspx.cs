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
	    public rollBack()
	    {
            CurrentApp = BusinessLogic.DefaultApps.content.ToString();

	    }
        private Document currentDoc = new Document(int.Parse(helper.Request("nodeId")));
        
        protected void version_load(object sender, EventArgs e) {

            if (allVersions.SelectedValue != "")
            {
                diffPanel.Visible = true;
                Document rollback = new Document(currentDoc.Id, new Guid(allVersions.SelectedValue));

                propertiesCompare.Text = "<tr><th>" + ui.Text("general", "name") + ":</th><td>" + rollback.Text + "</td></tr>";
                propertiesCompare.Text += "<tr><th>" + ui.Text("content", "createDate") + ":</th><td>" + rollback.VersionDate.ToLongDateString() + " " + rollback.VersionDate.ToLongTimeString() + " " + ui.Text("general", "by") + ": " + rollback.Writer.Name + "</td></tr>";

                if (rbl_mode.SelectedValue == "diff")
                    lt_notice.Text = ui.Text("rollback", "diffHelp");
                else
                    lt_notice.Text = ui.Text("rollback", "htmlHelp");


                var props = rollback.GenericProperties;
                foreach (Property p in props)
                {
                    try
                    {

                        if (p.Value != null)
                        {

                            //new property value... 
                            string thevalue = p.Value.ToString();

                            if (rbl_mode.SelectedValue == "diff")
                            {

                                //if display mode is set to diff...
                                thevalue = library.StripHtml(p.Value.ToString());
                                Property cP = currentDoc.getProperty(p.PropertyType);
                                if (cP != null && cP.Value != null)
                                {

                                    string cThevalue = library.StripHtml(cP.Value.ToString());

                                    propertiesCompare.Text += "<tr><th>" + p.PropertyType.Name + ":</th><td>" + library.ReplaceLineBreaks(cms.businesslogic.utilities.Diff.Diff2Html(cThevalue, thevalue)) + "</td></tr>";


                                }
                                else
                                {
                                    //If no current version of the value... display with no diff.
                                    propertiesCompare.Text += "<tr><th>" + p.PropertyType.Name + ":</th><td>" + thevalue + "</td></tr>";
                                }


                            }
                            else
                            {
                                //If display mode is html
                                propertiesCompare.Text += "<tr><th>" + p.PropertyType.Name + ":</th><td>" + thevalue + "</td></tr>";
                            }

                            //previewVersionContent.Controls.Add(new LiteralControl("<div style=\"margin-top: 4px; border: 1px solid #DEDEDE; padding: 4px;\"><p style=\"padding: 0px; margin: 0px;\" class=\"guiDialogNormal\"><b>" + p.PropertyType.Name + "</b><br/>"));
                            //previewVersionContent.Controls.Add(new LiteralControl(thevalue));

                        }
                        //previewVersionContent.Controls.Add(new LiteralControl("</p></div>"));
                    }
                    catch { }
                }

                Button1.Visible = true;
                

            }
            else
            {
                diffPanel.Visible = false;
                Button1.Visible = false;
            }

        }

		protected void Page_Load(object sender, System.EventArgs e)
		{

            if (String.IsNullOrEmpty(allVersions.SelectedValue))
                rbl_mode.AutoPostBack = false;
            else
                rbl_mode.AutoPostBack = true;

            currentVersionTitle.Text = currentDoc.Text;
            currentVersionMeta.Text = ui.Text("content", "createDate") + ": " + currentDoc.VersionDate.ToShortDateString() + " " + currentDoc.VersionDate.ToShortTimeString();

		    pp_selectVersion.Text = ui.Text("rollback", "headline");
		    pp_currentVersion.Text = ui.Text("rollback", "currentVersion");
		    pp_view.Text = ui.Text("rollback", "view");
		    pp_rollBackTo.Text = ui.Text("rollback", "rollbackTo");

            if (!IsPostBack) {
                allVersions.Items.Add(new ListItem(ui.Text("rollback", "selectVersion")+ "...", ""));
                
                foreach (DocumentVersionList dl in currentDoc.GetVersions())
                {
                    //we don't need to show the current version
                    if (dl.Version == currentDoc.Version)
                        continue;

                    allVersions.Items.Add(new ListItem(dl.Text + " (" + ui.Text("content", "createDate") + ": " + dl.Date.ToShortDateString() + " " + dl.Date.ToShortTimeString() + ")", dl.Version.ToString()));
                }
                Button1.Text = ui.Text("actions", "rollback");
            }
		}
        
		protected void doRollback_Click(object sender, System.EventArgs e)
		{
            if (allVersions.SelectedValue.Trim() != "")
            {
                Document d = new Document(int.Parse(helper.Request("nodeId")));
                d.RollBack(new Guid(allVersions.SelectedValue), base.getUser());
                
                BusinessLogic.Log.Add(BusinessLogic.LogTypes.RollBack, base.getUser(), d.Id, "Version rolled back to revision '" + allVersions.SelectedValue + "'");
                
                Document rollback = new Document(d.Id, new Guid(allVersions.SelectedValue));
                feedBackMsg.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
                string[] vars = {rollback.Text, rollback.VersionDate.ToLongDateString()};
                
                feedBackMsg.Text = ui.Text("rollback", "documentRolledBack", vars, new global::umbraco.BusinessLogic.User(0)) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
                diffPanel.Visible = false;
                pl_buttons.Visible = false;

                ClientTools.ReloadLocationIfMatched(string.Format("/content/content/edit/{0}", d.Id));
            }
		}
	}
}
