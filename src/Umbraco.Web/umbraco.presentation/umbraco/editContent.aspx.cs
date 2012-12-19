using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic.Actions;
using umbraco.IO;
using umbraco.uicontrols.DatePicker;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.presentation;
using System.Linq;
using Image = System.Web.UI.WebControls.Image;

namespace umbraco.cms.presentation
{
    public partial class editContent : BasePages.UmbracoEnsuredPage
    {
        protected uicontrols.TabView TabView1;
        protected System.Web.UI.WebControls.TextBox documentName;
        private cms.businesslogic.web.Document _document;
        private bool _documentHasPublishedVersion = false;
        protected System.Web.UI.WebControls.Literal jsIds;
        private LiteralControl dp = new LiteralControl();
        private DateTimePicker dpRelease = new DateTimePicker();
        private DateTimePicker dpExpire = new DateTimePicker();

        controls.ContentControl cControl;

        DropDownList ddlDefaultTemplate = new DropDownList();

        uicontrols.Pane publishProps = new uicontrols.Pane();
        uicontrols.Pane linkProps = new uicontrols.Pane();

        Button UnPublish = new Button();
        private Literal littPublishStatus = new Literal();

        private controls.ContentControl.publishModes _canPublish = controls.ContentControl.publishModes.Publish;

        private int? m_ContentId = null;

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //validate!
            int id;
            if (!int.TryParse(Request.QueryString["id"], out id))
            {
                //if this is invalid show an error
                this.DisplayFatalError("Invalid query string");
                return;
            }
            m_ContentId = id;


            this.UnPublish.Click += new System.EventHandler(this.UnPublishDo);

            //_document = new cms.businesslogic.web.Document(int.Parse(Request.QueryString["id"]));
            _document = new Document(true, id);

            //check if the doc exists
            if (string.IsNullOrEmpty(_document.Path))
            {
                //if this is invalid show an error
                this.DisplayFatalError("No document found with id " + m_ContentId);
                //reset the content id to null so processing doesn't continue on OnLoad
                m_ContentId = null;
                return;
            }

            // we need to check if there's a published version of this document
            _documentHasPublishedVersion = _document.HasPublishedVersion();

            // Check publishing permissions
            if (!base.getUser().GetPermissions(_document.Path).Contains(ActionPublish.Instance.Letter.ToString()))
            {
                // Check to see if the user has send to publish permissions
                if (!base.getUser().GetPermissions(_document.Path).Contains(ActionToPublish.Instance.Letter.ToString()))
                {
					//If no send to publish permission then revert to NoPublish mode
                    _canPublish = controls.ContentControl.publishModes.NoPublish;
                }
                else
                {
                    _canPublish = controls.ContentControl.publishModes.SendToPublish;
                }
            }

            cControl = new controls.ContentControl(_document, _canPublish, "TabView1");

            cControl.ID = "TabView1";

            cControl.Width = Unit.Pixel(666);
            cControl.Height = Unit.Pixel(666);

            // Add preview button

            foreach (uicontrols.TabPage tp in cControl.GetPanels())
            {
                addPreviewButton(tp.Menu, _document.Id);
            }

            plc.Controls.Add(cControl);


            System.Web.UI.WebControls.PlaceHolder publishStatus = new PlaceHolder();
            if (_documentHasPublishedVersion)
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
            cControl.PropertiesPane.addProperty(ui.Text("documentType"), new LiteralControl(DocumentType.Text));


            //template picker
            cControl.PropertiesPane.addProperty(ui.Text("template"), template);
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

            cControl.Save += new System.EventHandler(Save);
            cControl.SaveAndPublish += new System.EventHandler(Publish);
            cControl.SaveToPublish += new System.EventHandler(SendToPublish);

            // Add panes to property page...
            cControl.tpProp.Controls.AddAt(1, publishProps);
            cControl.tpProp.Controls.AddAt(2, linkProps);

            // add preview to properties pane too
            addPreviewButton(cControl.tpProp.Menu, _document.Id);


        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            if (!m_ContentId.HasValue)
                return;

            if (!CheckUserValidation())
                return;

            // clear preview cookie
            // zb-00004 #29956 : refactor cookies names & handling
            StateHelper.Cookies.Preview.Clear();

            if (!IsPostBack)
            {

                BusinessLogic.Log.Add(BusinessLogic.LogTypes.Open, base.getUser(), _document.Id, "");
                ClientTools.SyncTree(_document.Path, false);
            }


            jsIds.Text = "var umbPageId = " + _document.Id.ToString() + ";\nvar umbVersionId = '" + _document.Version.ToString() + "';\n";

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            UpdateNiceUrls();
        }

        protected void Save(object sender, System.EventArgs e)
        {
            // error handling test
            if (!Page.IsValid)
            {
                foreach (uicontrols.TabPage tp in cControl.GetPanels())
                {
                    tp.ErrorControl.Visible = true;
                    tp.ErrorHeader = ui.Text("errorHandling", "errorButDataWasSaved");
                    tp.CloseCaption = ui.Text("close");
                }
            }
            else if (Page.IsPostBack)
            {
                // hide validation summaries
                foreach (uicontrols.TabPage tp in cControl.GetPanels())
                {
                    tp.ErrorControl.Visible = false;
                }
            }
            //Audit trail...
            BusinessLogic.Log.Add(BusinessLogic.LogTypes.Save, base.getUser(), _document.Id, "");

            // Update name 
            if (_document.Text != cControl.NameTxt.Text)
            {
                //_refreshTree = true;
                _document.Text = cControl.NameTxt.Text;
                //newName.Text = _document.Text;
            }


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
            else
            {
                if (new DocumentType(_document.ContentType.Id).allowedTemplates.Length == 0)
                {
                    _document.RemoveTemplate();
                }
            }

            //The value of the properties has been set on IData through IDataEditor in the ContentControl
            //so we need to 'retrieve' that value and set it on the property of the new IContent object.
            //NOTE This is a workaround for the legacy approach to saving values through the DataType instead of the Property 
            //- (The DataType shouldn't be responsible for saving the value - especically directly to the db).
            foreach (var item in cControl.DataTypes)
            {
                _document.getProperty(item.Key).Value = item.Value.Data.Value;
            }

            // Run Handler				
            BusinessLogic.Actions.Action.RunActionHandlers(_document, ActionUpdate.Instance);
            _document.Save();

            // Update the update date
            dp.Text = _document.UpdateDate.ToShortDateString() + " " + _document.UpdateDate.ToShortTimeString();

            if (!cControl.DoesPublish)
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
                if (_document.Level == 1 || _document.PathPublished)
                {
                    Trace.Warn("before d.publish");

                    if (_document.PublishWithResult(base.getUser()))
                    {

                        ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editContentPublishedHeader", null), ui.Text("speechBubbles", "editContentPublishedText", null));
                        library.UpdateDocumentCache(_document);

                        littPublishStatus.Text = ui.Text("content", "lastPublished", base.getUser()) + ": " + _document.VersionDate.ToString() + "<br/>";

                        if (base.getUser().GetPermissions(_document.Path).IndexOf("U") > -1)
                            UnPublish.Visible = true;

                        _documentHasPublishedVersion = _document.HasPublishedVersion();
                    }
                    else
                    {
                        ClientTools.ShowSpeechBubble(speechBubbleIcon.warning, ui.Text("publish"), ui.Text("speechBubbles", "contentPublishedFailedByEvent"));
                    }
                }
                else
                    ClientTools.ShowSpeechBubble(speechBubbleIcon.warning, ui.Text("publish"), ui.Text("speechBubbles", "editContentPublishedFailedByParent"));

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
            _documentHasPublishedVersion = false;

            library.UnPublishSingleNode(_document.Id);

            //newPublishStatus.Text = "0";
        }

        void UpdateNiceUrlProperties(string niceUrlText, string altUrlsText)
        {
            Literal lit;

            linkProps.Controls.Clear();

            lit = new Literal();
            lit.Text = niceUrlText;
            linkProps.addProperty(ui.Text("content", "urls", base.getUser()), lit);

            if (!string.IsNullOrWhiteSpace(altUrlsText))
            {
                lit = new Literal();
                lit.Text = altUrlsText;
                linkProps.addProperty(ui.Text("content", "alternativeUrls", base.getUser()), lit);
            }
        }

        void UpdateNiceUrls()
        {
            if (!_documentHasPublishedVersion)
            {
                UpdateNiceUrlProperties("<i>" + ui.Text("content", "itemNotPublished", base.getUser()) + "</i>", null);
                return;
            }

            var niceUrlProvider = Umbraco.Web.UmbracoContext.Current.RoutingContext.NiceUrlProvider;
            var url = niceUrlProvider.GetNiceUrl(_document.Id);
            string niceUrlText = null;
            var altUrlsText = new System.Text.StringBuilder();

            if (url == "#")
            {
                // document as a published version yet it's url is "#" => a parent must be
                // unpublished, walk up the tree until we find it, and report.
                var parent = _document;
                do
                {
                    parent = parent.ParentId > 0 ? new Document(parent.ParentId) : null;
                }
                while (parent != null && parent.Published);

                if (parent == null) // oops - internal error
                    niceUrlText = "<i>" + ui.Text("content", "parentNotPublished", "???", base.getUser()) + "</i>";
                else
                    niceUrlText = "<i>" + ui.Text("content", "parentNotPublished", parent.Text, base.getUser()) + "</i>";
            }
            else
            {
                niceUrlText = string.Format("<a href=\"{0}\" target=\"_blank\">{0}</a>", url);

                foreach (var altUrl in niceUrlProvider.GetAllAbsoluteNiceUrls(_document.Id).Where(u => u != url))
                    altUrlsText.AppendFormat("<a href=\"{0}\" target=\"_blank\">{0}</a><br />", altUrl);
            }

            UpdateNiceUrlProperties(niceUrlText, altUrlsText.ToString());
        }

        /// <summary>
        /// Clears the page of all controls and shows a simple message. Used if users don't have visible access to the page.
        /// </summary>
        /// <param name="message"></param>        
        private void ShowUserValidationError(string message)
        {
            this.Controls.Clear();
            this.Controls.Add(new LiteralControl(String.Format("<link rel='stylesheet' type='text/css' href='../../umbraco_client/ui/default.css'><link rel='stylesheet' type='text/css' href='../../umbraco_client/tabview/style.css'><link rel='stylesheet' type='text/css' href='../../umbraco_client/propertypane/style.css'><div id='body_dashboardTabs' style='height: auto; width: auto;'><div class='header'><ul><li id='body_dashboardTabs_tab01' class='tabOn'><a id='body_dashboardTabs_tab01a' href='#' onclick='setActiveTab('body_dashboardTabs','body_dashboardTabs_tab01',body_dashboardTabs_tabs); return false;'><span><nobr>Access denied</nobr></span></a></li></ul></div><div id='' class='tabpagecontainer'><div id='body_dashboardTabs_tab01layer' class='tabpage' style='display: block;'><div class='menubar'></div><div class='tabpagescrollinglayer' id='body_dashboardTabs_tab01layer_contentlayer' style='width: auto; height: auto;'><div class='tabpageContent' style='padding:0 10px;'><div class='propertypane' style=''><div><div class='propertyItem' style=''><div class='dashboardWrapper'><h2>Access denied</h2><img src='./dashboard/images/access-denied.png' alt='Access denied' class='dashboardIcon'>{0}</div></div></div></div></div></div></div></div><div class='footer'><div class='status'><h2></h2></div></div></div>", message)));
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
                ShowUserValidationError("<h3>The current user doesn't have access to this application</h3><p>Please contact the system administrator if you think that you should have access.</p>");
                return false;
            }
            if (!base.ValidateUserNodeTreePermissions(_document.Path, ActionBrowse.Instance.Letter.ToString()))
            {
                ShowUserValidationError(
                    "<h3>The current user doesn't have permissions to browse this document</h3><p>Please contact the system administrator if you think that you should have access.</p>");
                return false;
            }
            //TODO: Change this, when we add view capabilities, the user will be able to view but not edit!
            if (!base.ValidateUserNodeTreePermissions(_document.Path, ActionUpdate.Instance.Letter.ToString()))
            {
                ShowUserValidationError("<h3>The current user doesn't have permissions to edit this document</h3><p>Please contact the system administrator if you think that you should have access.</p>");
                return false;
            }
            return true;
        }

        private void addPreviewButton(uicontrols.ScrollingMenu menu, int id)
        {
            uicontrols.MenuIconI menuItem;

            // Find the first splitter in the Menu - Should be the rte toolbar's splitter
            var startIndex = menu.FindSplitter(1);
            
            if (startIndex == -1)
            {
                // No Splitter found - rte toolbar isn't loaded
            menu.InsertSplitter();
                menuItem = menu.NewIcon();
            } 
            else
            {
                // Rte toolbar is loaded, inject after it's Splitter
                menuItem = menu.NewIcon(startIndex + 1);
                menu.InsertSplitter(startIndex + 2);
            }
            
            menuItem.ImageURL = SystemDirectories.Umbraco + "/images/editor/vis.gif";
            // Fix for U4-682, if there's no template, disable the preview button
            if (_document.Template != -1)
            {
                menuItem.AltText = ui.Text("buttons", "showPage", this.getUser());
                menuItem.OnClickCommand = "window.open('dialogs/preview.aspx?id=" + id + "','umbPreview')";
            }
            else
            {
                string showPageDisabledText = ui.Text("buttons", "showPageDisabled", this.getUser());
                if (showPageDisabledText.StartsWith("["))
                    showPageDisabledText = ui.GetText("buttons", "showPageDisabled", null, "en"); ;

                menuItem.AltText = showPageDisabledText;
                ((Image) menuItem).Attributes.Add("style", "opacity: 0.5");
            }
        }

    }
}
