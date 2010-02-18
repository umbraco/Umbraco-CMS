using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Reflection;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.helpers;
using umbraco.IO;
using umbraco.uicontrols.DatePicker;


namespace umbraco.cms.presentation
{
    public partial class editContent : BasePages.UmbracoEnsuredPage
    {
        protected uicontrols.TabView TabView1;
        protected System.Web.UI.WebControls.TextBox documentName;
        private cms.businesslogic.web.Document _document;
        protected System.Web.UI.WebControls.Literal jsIds;
        
        /*
        private controls.datePicker dp = new controls.datePicker();
        private controls.datePicker dpRelease = new controls.datePicker();
        private controls.datePicker dpExpire = new controls.datePicker();
        */

        private LiteralControl dp = new LiteralControl();
        private DateTimePicker dpRelease = new DateTimePicker();
        private DateTimePicker dpExpire = new DateTimePicker();

        //private bool _refreshTree = false;

        controls.ContentControl tmp;

        DropDownList ddlDefaultTemplate = new DropDownList();

        uicontrols.Pane publishProps = new uicontrols.Pane();
        uicontrols.Pane linkProps = new uicontrols.Pane();

        Button UnPublish = new Button();
        private Literal littPublishStatus = new Literal();

        private Literal l = new Literal();
        private Literal domainText = new Literal();

        //protected System.Web.UI.WebControls.Literal SyncPath;

        private controls.ContentControl.publishModes _canPublish = controls.ContentControl.publishModes.Publish;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            if (!CheckUserValidation())
                return;

			//if (helper.Request("frontEdit") != "")
			//    syncScript.Visible = false;

            if (!IsPostBack)
            {
                //SyncPath.Text = _document.Path;
				//newName.Text = _document.Text.Replace("'", "\\'");
                //_refreshTree = true;
                BusinessLogic.Log.Add(BusinessLogic.LogTypes.Open, base.getUser(), _document.Id, "");
				ClientTools.SyncTree(_document.Path, false);
            }
            else
            {
                // by default, don't refresh the tree on postbacks
                //_refreshTree = false;
            }

            jsIds.Text = "var umbPageId = " + _document.Id.ToString() + ";\nvar umbVersionId = '" + _document.Version.ToString() + "';\n";

        }

        protected void Save(object sender, System.EventArgs e)
        {
            // error handling test
            if (!Page.IsValid)
            {
                foreach (uicontrols.TabPage tp in tmp.GetPanels())
                {
                    tp.ErrorControl.Visible = true;
                    tp.ErrorHeader = ui.Text("errorHandling", "errorButDataWasSaved");
                    tp.CloseCaption = ui.Text("close");
                }
            } else if (Page.IsPostBack) {
                // hide validation summaries
                foreach (uicontrols.TabPage tp in tmp.GetPanels())
                {
                    tp.ErrorControl.Visible = false;
                }
            }
                //Audit trail...
                BusinessLogic.Log.Add(BusinessLogic.LogTypes.Save, base.getUser(), _document.Id, "");

                // Update name 
                if (_document.Text != tmp.NameTxt.Text)
                {
                    //_refreshTree = true;
                    _document.Text = tmp.NameTxt.Text;
                    //newName.Text = _document.Text;
                }

                // Update the update date
                _document.UpdateDate = System.DateTime.Now;
                dp.Text = _document.UpdateDate.ToShortDateString() + " " + _document.UpdateDate.ToShortTimeString();
                if (dpRelease.DateTime > new DateTime(1753, 1, 1) && dpRelease.DateTime < new DateTime(9999, 12, 31))
                    _document.ReleaseDate = dpRelease.DateTime;
                else
                    _document.ReleaseDate = new DateTime(1, 1, 1, 0, 0, 0);
                if (dpExpire.DateTime > new DateTime(1753, 1, 1) && dpExpire.DateTime < new DateTime(9999, 12, 31))
                    _document.ExpireDate = dpExpire.DateTime;
                else
                    _document.ExpireDate = new DateTime(1, 1, 1, 0, 0, 0);

                // Update default template
                if (ddlDefaultTemplate.SelectedIndex > 0)
                {
                    _document.Template = int.Parse(ddlDefaultTemplate.SelectedValue);
                }

                // Run Handler				
                BusinessLogic.Actions.Action.RunActionHandlers(_document, ActionUpdate.Instance);
                _document.Save();

                if (!tmp.DoesPublish)
                    ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editContentSavedHeader", null), ui.Text("speechBubbles", "editContentSavedText", null));

				ClientTools.SyncTree(_document.Path, true);
        }

        protected void SendToPublish(object sender, System.EventArgs e)
        {
            if (Page.IsValid)
            {
                ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editContentSendToPublish", base.getUser()), ui.Text("speechBubbles", "editContentSendToPublishText", base.getUser()));
                _document.SendToPublication(base.getUser());
            }
        }

        protected void Publish(object sender, System.EventArgs e)
        {
            if (Page.IsValid)
            {
                if (_document.Level == 1 || new cms.businesslogic.web.Document(_document.Parent.Id).Published)
                {
                    Trace.Warn("before d.publish");

                    // Refresh tree as the document publishing status changes (we'll always update the tree on publish as the icon can have been marked with the star indicating that the content of the page have been changed)
                    //_refreshTree = true;
                    //newPublishStatus.Text = "1";

                    if (_document.PublishWithResult(base.getUser()))
                    {
                        ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editContentPublishedHeader", null), ui.Text("speechBubbles", "editContentPublishedText", null));
                        library.UpdateDocumentCache(_document.Id);

                        BusinessLogic.Log.Add(BusinessLogic.LogTypes.Publish, base.getUser(), _document.Id, "");
                        littPublishStatus.Text = ui.Text("content", "lastPublished", base.getUser()) + ": " + _document.VersionDate.ToString() + "<br/>";
                        
                        if (base.getUser().GetPermissions(_document.Path).IndexOf("U") > -1)
                            UnPublish.Visible = true;

                        updateLinks();
                    }
                    else
                    {
                        ClientTools.ShowSpeechBubble(speechBubbleIcon.error, ui.Text("error"), ui.Text("contentPublishedFailedByEvent"));
                    }
                }
                else
                    ClientTools.ShowSpeechBubble(speechBubbleIcon.error, ui.Text("error"), ui.Text("speechBubbles", "editContentPublishedFailedByParent"));
                
                // page cache disabled...
                //			cms.businesslogic.cache.Cache.ClearCacheObjectTypes("umbraco.page");


                // Update links
            }
        }

        protected void UnPublishDo(object sender, System.EventArgs e)
        {
            _document.UnPublish();
            littPublishStatus.Text = ui.Text("content", "itemNotPublished", base.getUser());
            UnPublish.Visible = false;

            library.UnPublishSingleNode(_document.Id);

            // the treeview should be updated to reflect changes
            //_refreshTree = true;
            //newPublishStatus.Text = "0";

        }

        private void updateLinks()
        {
            if (_document.Published)
            {
                l.Text = "<a href=\"" + library.NiceUrl(_document.Id) + "\" target=\"_blank\">" + library.NiceUrl(_document.Id) + "</a>";

                string currentLink = library.NiceUrl(_document.Id);

                // domains
                domainText.Text = "";
                foreach (string s in _document.Path.Split(','))
                {
                    if (int.Parse(s) > -1)
                    {
                        cms.businesslogic.web.Document dChild = new cms.businesslogic.web.Document(int.Parse(s));
                        if (dChild.Published)
                        {
                            cms.businesslogic.web.Domain[] domains = cms.businesslogic.web.Domain.GetDomainsById(int.Parse(s));
                            if (domains.Length > 0)
                            {
                                for (int i = 0; i < domains.Length; i++)
                                {
                                    string tempLink = "";
                                    if (library.NiceUrl(int.Parse(s)) == "")
                                        tempLink = "<em>N/A</em>";
                                    else if (int.Parse(s) != _document.Id)
                                        tempLink = "http://" + domains[i].Name + currentLink.Replace(library.NiceUrl(int.Parse(s)).Replace(".aspx", ""), "");
                                    else
                                        tempLink = "http://" + domains[i].Name;

                                    domainText.Text += "<a href=\"" + tempLink + "\" target=\"_blank\">" + tempLink + "</a><br/>";
                                }
                            }
                        }
                        else
                            l.Text = "<i>" + ui.Text("content", "parentNotPublished", dChild.Text, base.getUser()) + "</i>";
                    }
                }

            }
            else
                l.Text = "<i>" + ui.Text("content", "itemNotPublished", base.getUser()) + "</i>";
        }

        /// <summary>
        /// Clears the page of all controls and shows a simple message. Used if users don't have visible access to the page.
        /// </summary>
        /// <param name="message"></param>        
        private void ShowUserValidationError(string message)
        {
            this.Controls.Clear();
            this.Controls.Add(new LiteralControl(String.Format("<h1>{0}</h1>", message)));
        }

        /// <summary>
        /// Checks if the user cannot view/browse this page/app and displays an html message to the user if this is not the case.
        /// </summary>
        /// <returns></returns>
        private bool CheckUserValidation()
        {
            // Validate permissions
            if (!base.ValidateUserApp("content"))
            {
                ShowUserValidationError("The current user doesn't have access to this application. Please contact the system administrator.");
                return false;
            }
            if (!base.ValidateUserNodeTreePermissions(_document.Path, ActionBrowse.Instance.Letter.ToString()))
            {
                ShowUserValidationError("The current user doesn't have permissions to browse this document. Please contact the system administrator.");
                return false;
            }
            //TODO: Change this, when we add view capabilities, the user will be able to view but not edit!
            if (!base.ValidateUserNodeTreePermissions(_document.Path, ActionUpdate.Instance.Letter.ToString()))
            {
                ShowUserValidationError("The current user doesn't have permissions to edit this document. Please contact the system administrator.");
                return false;
            }
            return true;
        }


        /// <summary>
        /// If the user has permissions to update, then add the template drop down list, otherwise just write out the template that is currently used.
        /// </summary>
        /// <param name="defaultTemplate"></param>
        /// <param name="template"></param>
        /// <param name="DocumentType"></param>
        private void AddTemplateDropDown(int defaultTemplate, ref PlaceHolder template, ref cms.businesslogic.web.DocumentType DocumentType)
        {
            string permissions = this.getUser().GetPermissions(_document.Path);

            if (permissions.IndexOf(ActionUpdate.Instance.Letter) >= 0)
            {
                ddlDefaultTemplate.Items.Add(new ListItem(ui.Text("choose") + "...", ""));
                foreach (cms.businesslogic.template.Template t in DocumentType.allowedTemplates)
                {
                    ListItem tTemp = new ListItem(t.Text, t.Id.ToString());
                    if (t.Id == defaultTemplate)
                        tTemp.Selected = true;
                    ddlDefaultTemplate.Items.Add(tTemp);
                }


                template.Controls.Add(ddlDefaultTemplate);
            }
            else
            {
                if (defaultTemplate != 0)
                    template.Controls.Add(new LiteralControl(new cms.businesslogic.template.Template(defaultTemplate).Text));
                else
                    template.Controls.Add(new LiteralControl(ui.Text("content", "noDefaultTemplate")));
            }
        }

		//protected override void OnPreRender(EventArgs e)
		//{
		//    base.OnPreRender(e);
		//    if (_refreshTree && _document != null)
		//    {
		//        ClientTools.SyncTree(_document.Path);
		//    }

		//    //syncScript.Visible = _refreshTree;
		//}

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            base.OverrideClientTarget = false;

            InitializeComponent();
            base.OnInit(e);

            _document = new cms.businesslogic.web.Document(int.Parse(Request.QueryString["id"]));

            // Check publishing permissions
            if (!base.getUser().GetPermissions(_document.Path).Contains(ActionPublish.Instance.Letter.ToString()))
                _canPublish = controls.ContentControl.publishModes.SendToPublish;
            tmp = new controls.ContentControl(_document, _canPublish, "TabView1");

            tmp.ID = "TabView1";

            tmp.Width = Unit.Pixel(666);
            tmp.Height = Unit.Pixel(666);

            // Add preview button

            foreach (uicontrols.TabPage tp in tmp.GetPanels())
            {
                tp.Menu.InsertSplitter(2);
                uicontrols.MenuIconI menuItem = tp.Menu.NewIcon(3);
                menuItem.AltText = ui.Text("buttons", "showPage", this.getUser());
                menuItem.OnClickCommand = "window.open('dialogs/preview.aspx?id=" + Request["id"] + "','umbPreview')";
                menuItem.ImageURL = SystemDirectories.Umbraco + "/images/editor/vis.gif";
                //				tp.Menu.InsertSplitter(4);
            }

            plc.Controls.Add(tmp);


            System.Web.UI.WebControls.PlaceHolder publishStatus = new PlaceHolder();
            if (_document.Published)
            {
                littPublishStatus.Text = ui.Text("content", "lastPublished", base.getUser()) + ": " + _document.VersionDate.ToShortDateString() + " &nbsp; ";

                publishStatus.Controls.Add(littPublishStatus);
                if (base.getUser().GetPermissions(_document.Path).IndexOf("U") > -1)
                    UnPublish.Visible = true;
                else
                    UnPublish.Visible = false;
            }
            else
            {
                littPublishStatus.Text = ui.Text("content", "itemNotPublished", base.getUser());
                publishStatus.Controls.Add(littPublishStatus);
                UnPublish.Visible = false;
            }

            UnPublish.Text = ui.Text("content", "unPublish", base.getUser());
            UnPublish.ID = "UnPublishButton";
            UnPublish.Attributes.Add("onClick", "if (!confirm('" + ui.Text("defaultdialogs", "confirmSure", base.getUser()) + "')) return false; ");
            publishStatus.Controls.Add(UnPublish);

            publishProps.addProperty(ui.Text("content", "publishStatus", base.getUser()), publishStatus);

            // Template
            PlaceHolder template = new PlaceHolder();
            cms.businesslogic.web.DocumentType DocumentType = new cms.businesslogic.web.DocumentType(_document.ContentType.Id);
            tmp.PropertiesPane.addProperty(ui.Text("documentType"), new LiteralControl(DocumentType.Text));
            tmp.PropertiesPane.addProperty(ui.Text("template"), template);

            int defaultTemplate;
            if (_document.Template != 0)
                defaultTemplate = _document.Template;
            else
                defaultTemplate = DocumentType.DefaultTemplate;

            if (this.getUser().UserType.Name == "writer")
            {
                if (defaultTemplate != 0)
                    template.Controls.Add(new LiteralControl(cms.businesslogic.template.Template.GetTemplate(defaultTemplate).Text));
                else
                    template.Controls.Add(new LiteralControl(ui.Text("content", "noDefaultTemplate")));
            }
            else
            {
                ddlDefaultTemplate.Items.Add(new ListItem(ui.Text("choose") + "...", ""));
                foreach (cms.businesslogic.template.Template t in DocumentType.allowedTemplates)
                {
                    ListItem tTemp = new ListItem(t.Text, t.Id.ToString());
                    if (t.Id == defaultTemplate)
                        tTemp.Selected = true;
                    ddlDefaultTemplate.Items.Add(tTemp);
                }
                template.Controls.Add(ddlDefaultTemplate);
            }


            // Editable update date, release date and expire date added by NH 13.12.04
            dp.ID = "updateDate";
            dp.Text = _document.UpdateDate.ToShortDateString() + " " + _document.UpdateDate.ToShortTimeString();
            publishProps.addProperty(ui.Text("content", "updateDate", base.getUser()), dp);

            dpRelease.ID = "releaseDate";
            dpRelease.DateTime = _document.ReleaseDate;
            dpRelease.ShowTime = true;
            publishProps.addProperty(ui.Text("content", "releaseDate", base.getUser()), dpRelease);

            dpExpire.ID = "expireDate";
            dpExpire.DateTime = _document.ExpireDate;
            dpExpire.ShowTime = true;
            publishProps.addProperty(ui.Text("content", "expireDate", base.getUser()), dpExpire);

            // url's
            updateLinks();
            linkProps.addProperty(ui.Text("content", "urls", base.getUser()), l);

            if (domainText.Text != "")
                linkProps.addProperty(ui.Text("content", "alternativeUrls", base.getUser()), domainText);

            tmp.Save += new System.EventHandler(Save);
            tmp.SaveAndPublish += new System.EventHandler(Publish);
            tmp.SaveToPublish += new System.EventHandler(SendToPublish);

            // Add panes to property page...
            tmp.tpProp.Controls.Add(publishProps);
            tmp.tpProp.Controls.Add(linkProps);


        }

        private void InitializeComponent()
        {
            this.UnPublish.Click += new System.EventHandler(this.UnPublishDo);
        }
        #endregion
    }
}
