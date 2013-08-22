using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.IO;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using umbraco.BusinessLogic.Actions;
using umbraco.uicontrols.DatePicker;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.presentation;
using System.Linq;
using Image = System.Web.UI.WebControls.Image;
using Umbraco.Core;

namespace umbraco.cms.presentation
{
    public class editContent : BasePages.UmbracoEnsuredPage
    {
        protected uicontrols.TabView TabView1;
        protected TextBox documentName;
        private Document _document;
        private bool _documentHasPublishedVersion = false;
        protected Literal jsIds;
        private readonly LiteralControl _dp = new LiteralControl();
        private readonly DateTimePicker _dpRelease = new DateTimePicker();
        private readonly DateTimePicker _dpExpire = new DateTimePicker();

        private controls.ContentControl _cControl;

        private readonly DropDownList _ddlDefaultTemplate = new DropDownList();

        private readonly uicontrols.Pane _publishProps = new uicontrols.Pane();
        private readonly uicontrols.Pane _linkProps = new uicontrols.Pane();

        private readonly Button _unPublish = new Button();
        private readonly Literal _littPublishStatus = new Literal();

        private controls.ContentControl.publishModes _canPublish = controls.ContentControl.publishModes.Publish;

        private int? _contentId = null;

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //validate!
            int id;
            if (int.TryParse(Request.QueryString["id"], out id) == false)
            {
                //if this is invalid show an error
                this.DisplayFatalError("Invalid query string");
                return;
            }
            _contentId = id;


            _unPublish.Click += UnPublishDo;

            //Loading Content via new public service to ensure that the Properties are loaded correct
            var content = ApplicationContext.Current.Services.ContentService.GetById(id);
            _document = new Document(content);

            //check if the doc exists
            if (string.IsNullOrEmpty(_document.Path))
            {
                //if this is invalid show an error
                this.DisplayFatalError("No document found with id " + _contentId);
                //reset the content id to null so processing doesn't continue on OnLoad
                _contentId = null;
                return;
            }

            // we need to check if there's a published version of this document
            _documentHasPublishedVersion = _document.Content.HasPublishedVersion();

            // Check publishing permissions
            if (!UmbracoUser.GetPermissions(_document.Path).Contains(ActionPublish.Instance.Letter.ToString()))
            {
                // Check to see if the user has send to publish permissions
                if (!UmbracoUser.GetPermissions(_document.Path).Contains(ActionToPublish.Instance.Letter.ToString()))
                {
                    //If no send to publish permission then revert to NoPublish mode
                    _canPublish = controls.ContentControl.publishModes.NoPublish;
                }
                else
                {
                    _canPublish = controls.ContentControl.publishModes.SendToPublish;
                }
            }

            _cControl = new controls.ContentControl(_document, _canPublish, "TabView1");

            _cControl.ID = "TabView1";

            _cControl.Width = Unit.Pixel(666);
            _cControl.Height = Unit.Pixel(666);

            // Add preview button

            foreach (uicontrols.TabPage tp in _cControl.GetPanels())
            {
                AddPreviewButton(tp.Menu, _document.Id);
            }

            plc.Controls.Add(_cControl);


            var publishStatus = new PlaceHolder();
            if (_documentHasPublishedVersion)
            {
                _littPublishStatus.Text = ui.Text("content", "lastPublished", UmbracoUser) + ": " + _document.VersionDate.ToShortDateString() + " &nbsp; ";

                publishStatus.Controls.Add(_littPublishStatus);
                if (UmbracoUser.GetPermissions(_document.Path).IndexOf("U") > -1)
                    _unPublish.Visible = true;
                else
                    _unPublish.Visible = false;
            }
            else
            {
                _littPublishStatus.Text = ui.Text("content", "itemNotPublished", UmbracoUser);
                publishStatus.Controls.Add(_littPublishStatus);
                _unPublish.Visible = false;
            }

            _unPublish.Text = ui.Text("content", "unPublish", UmbracoUser);
            _unPublish.ID = "UnPublishButton";
            _unPublish.Attributes.Add("onClick", "if (!confirm('" + ui.Text("defaultdialogs", "confirmSure", UmbracoUser) + "')) return false; ");
            publishStatus.Controls.Add(_unPublish);

            _publishProps.addProperty(ui.Text("content", "publishStatus", UmbracoUser), publishStatus);

            // Template
            var template = new PlaceHolder();
            var documentType = new DocumentType(_document.ContentType.Id);
            _cControl.PropertiesPane.addProperty(ui.Text("documentType"), new LiteralControl(documentType.Text));


            //template picker
            _cControl.PropertiesPane.addProperty(ui.Text("template"), template);
            int defaultTemplate;
            if (_document.Template != 0)
                defaultTemplate = _document.Template;
            else
                defaultTemplate = documentType.DefaultTemplate;

            if (UmbracoUser.UserType.Name == "writer")
            {
                if (defaultTemplate != 0)
                    template.Controls.Add(new LiteralControl(businesslogic.template.Template.GetTemplate(defaultTemplate).Text));
                else
                    template.Controls.Add(new LiteralControl(ui.Text("content", "noDefaultTemplate")));
            }
            else
            {
                _ddlDefaultTemplate.Items.Add(new ListItem(ui.Text("choose") + "...", ""));
                foreach (var t in documentType.allowedTemplates)
                {

                    var tTemp = new ListItem(t.Text, t.Id.ToString());
                    if (t.Id == defaultTemplate)
                        tTemp.Selected = true;
                    _ddlDefaultTemplate.Items.Add(tTemp);
                }
                template.Controls.Add(_ddlDefaultTemplate);
            }


            // Editable update date, release date and expire date added by NH 13.12.04
            _dp.ID = "updateDate";
            _dp.Text = _document.UpdateDate.ToShortDateString() + " " + _document.UpdateDate.ToShortTimeString();
            _publishProps.addProperty(ui.Text("content", "updateDate", UmbracoUser), _dp);

            _dpRelease.ID = "releaseDate";
            _dpRelease.DateTime = _document.ReleaseDate;
            _dpRelease.ShowTime = true;
            _publishProps.addProperty(ui.Text("content", "releaseDate", UmbracoUser), _dpRelease);

            _dpExpire.ID = "expireDate";
            _dpExpire.DateTime = _document.ExpireDate;
            _dpExpire.ShowTime = true;
            _publishProps.addProperty(ui.Text("content", "expireDate", UmbracoUser), _dpExpire);

            _cControl.Save += Save;
            _cControl.SaveAndPublish += Publish;
            _cControl.SaveToPublish += SendToPublish;

            // Add panes to property page...
            _cControl.tpProp.Controls.AddAt(1, _publishProps);
            _cControl.tpProp.Controls.AddAt(2, _linkProps);

            // add preview to properties pane too
            AddPreviewButton(_cControl.tpProp.Menu, _document.Id);


        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!_contentId.HasValue)
                return;

            if (!CheckUserValidation())
                return;

            // clear preview cookie
            // zb-00004 #29956 : refactor cookies names & handling
            StateHelper.Cookies.Preview.Clear();

            if (!IsPostBack)
            {

                Log.Add(LogTypes.Open, UmbracoUser, _document.Id, "");
                ClientTools.SyncTree(_document.Path, false);
            }


            jsIds.Text = "var umbPageId = " + _document.Id.ToString() + ";\nvar umbVersionId = '" + _document.Version.ToString() + "';\n";

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            UpdateNiceUrls();
        }

        /// <summary>
        /// Handles the Save event for the ContentControl.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This will set the document's properties and persist a new document revision
        /// </remarks>
        protected void Save(object sender, EventArgs e)
        {
            //NOTE: This is only here because we have to keep backwards compatibility with events in the ContentControl.
            // see: http://issues.umbraco.org/issue/U4-1660
            // in this case both Save and SaveAndPublish will fire when we are publishing but we only want to handle that once,
            // so if this is actually doing a publish, we'll exit and rely on the SaveAndPublish handler to do all the work.
            if (_cControl.DoesPublish)
            {
                return;
            }

            //update UI and set document properties
            PerformSaveLogic();

            //persist the document
            _document.Save();

            // Run Handler				
            BusinessLogic.Actions.Action.RunActionHandlers(_document, ActionUpdate.Instance);

            ClientTools.ShowSpeechBubble(
                speechBubbleIcon.save, ui.Text("speechBubbles", "editContentSavedHeader", null),
                ui.Text("speechBubbles", "editContentSavedText", null));

            ClientTools.SyncTree(_document.Path, true);
        }

        /// <summary>
        /// Handles the SendToPublish event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SendToPublish(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                ClientTools.ShowSpeechBubble(
                    speechBubbleIcon.save, ui.Text("speechBubbles", "editContentSendToPublish", UmbracoUser),
                    ui.Text("speechBubbles", "editContentSendToPublishText", UmbracoUser));
                _document.SendToPublication(UmbracoUser);
            }
        }

        /// <summary>
        /// Handles the SaveAndPublish event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Sets the document's properties and if the page is valid continues to publish it, otherwise just saves a revision.
        /// </remarks>
        protected void Publish(object sender, EventArgs e)
        {
            //update UI and set document properties
            PerformSaveLogic();

            //the business logic here will check to see if the doc can actually be published and will return the 
            // appropriate result so we can display the correct error messages (or success).
            var savePublishResult = _document.SaveAndPublishWithResult(UmbracoUser);

            ShowMessageForStatus(savePublishResult.Result);

            if (savePublishResult.Success)
            {
                _littPublishStatus.Text = string.Format("{0}: {1}<br/>", ui.Text("content", "lastPublished", UmbracoUser), _document.VersionDate.ToString());

                if (UmbracoUser.GetPermissions(_document.Path).IndexOf("U") > -1)
                {
                    _unPublish.Visible = true;
                }

                _documentHasPublishedVersion = _document.Content.HasPublishedVersion();
            }

            ClientTools.SyncTree(_document.Path, true);
        }

        private void ShowMessageForStatus(PublishStatus status)
        {
            switch (status.StatusType)
            {
                case PublishStatusType.Success:
                case PublishStatusType.SuccessAlreadyPublished:
                    ClientTools.ShowSpeechBubble(
                        speechBubbleIcon.save,
                        ui.Text("speechBubbles", "editContentPublishedHeader", UmbracoUser),
                        ui.Text("speechBubbles", "editContentPublishedText", UmbracoUser));
                    break;
                case PublishStatusType.FailedPathNotPublished:
                    ClientTools.ShowSpeechBubble(
                        speechBubbleIcon.warning,
                        ui.Text("publish"),
                        ui.Text("publish", "contentPublishedFailedByParent",
                                string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id),
                                UmbracoUser).Trim());
                    break;
                case PublishStatusType.FailedCancelledByEvent:
                    ClientTools.ShowSpeechBubble(
                        speechBubbleIcon.warning,
                        ui.Text("publish"),
                        ui.Text("speechBubbles", "contentPublishedFailedByEvent"));
                    break;
                case PublishStatusType.FailedHasExpired:
                case PublishStatusType.FailedAwaitingRelease:
                case PublishStatusType.FailedIsTrashed:
                case PublishStatusType.FailedContentInvalid:
                    ClientTools.ShowSpeechBubble(
                        speechBubbleIcon.warning,
                        ui.Text("publish"),
                        ui.Text("publish", "contentPublishedFailedInvalid",
                                new[]
                                    {
                                        string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id),
                                        string.Join(",", status.InvalidProperties.Select(x => x.Alias))
                                    },
                                UmbracoUser).Trim());
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        protected void UnPublishDo(object sender, EventArgs e)
        {
            _document.UnPublish();
            _littPublishStatus.Text = ui.Text("content", "itemNotPublished", UmbracoUser);
            _unPublish.Visible = false;
            _documentHasPublishedVersion = false;

            //library.UnPublishSingleNode(_document.Id);

            Current.ClientTools.SyncTree(_document.Path, true);
            ClientTools.ShowSpeechBubble(speechBubbleIcon.success, ui.Text("unpublish"), ui.Text("speechBubbles", "contentUnpublished"));

            //newPublishStatus.Text = "0";
        }

        void UpdateNiceUrlProperties(string niceUrlText, string altUrlsText)
        {
            _linkProps.Controls.Clear();

            var lit = new Literal();
            lit.Text = niceUrlText;
            _linkProps.addProperty(ui.Text("content", "urls", UmbracoUser), lit);

            if (!string.IsNullOrWhiteSpace(altUrlsText))
            {
                lit = new Literal();
                lit.Text = altUrlsText;
                _linkProps.addProperty(ui.Text("content", "alternativeUrls", UmbracoUser), lit);
            }
        }

        void UpdateNiceUrls()
        {
            if (_documentHasPublishedVersion == false)
            {
                UpdateNiceUrlProperties("<i>" + ui.Text("content", "itemNotPublished", UmbracoUser) + "</i>", null);
                return;
            }

            var urlProvider = Umbraco.Web.UmbracoContext.Current.RoutingContext.UrlProvider;
            var url = urlProvider.GetUrl(_document.Id);
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
                    niceUrlText = "<i>" + ui.Text("content", "parentNotPublishedAnomaly", UmbracoUser) + "</i>";
                else
                    niceUrlText = "<i>" + ui.Text("content", "parentNotPublished", parent.Text, UmbracoUser) + "</i>";
            }
            else
            {
                niceUrlText = string.Format("<a href=\"{0}\" target=\"_blank\">{0}</a>", url);

                foreach (var otherUrl in urlProvider.GetOtherUrls(_document.Id))
                    altUrlsText.AppendFormat("<a href=\"{0}\" target=\"_blank\">{0}</a><br />", otherUrl);
            }

            UpdateNiceUrlProperties(niceUrlText, altUrlsText.ToString());
        }

        /// <summary>
        /// When a document is saved or published all of this logic must be performed.
        /// </summary>
        /// <remarks>
        /// This updates both UI controls and business logic object properties but does not persist any data to 
        /// business logic repositories.
        /// </remarks>
        private void PerformSaveLogic()
        {
            // error handling test
            if (!Page.IsValid)
            {
                foreach (uicontrols.TabPage tp in _cControl.GetPanels())
                {
                    tp.ErrorControl.Visible = true;
                    tp.ErrorHeader = ui.Text("errorHandling", "errorButDataWasSaved");
                    tp.CloseCaption = ui.Text("close");
                }
            }
            else if (Page.IsPostBack)
            {
                // hide validation summaries
                foreach (uicontrols.TabPage tp in _cControl.GetPanels())
                {
                    tp.ErrorControl.Visible = false;
                }
            }

            if (_dpRelease.DateTime > new DateTime(1753, 1, 1) && _dpRelease.DateTime < new DateTime(9999, 12, 31))
                _document.ReleaseDate = _dpRelease.DateTime;
            else
                _document.ReleaseDate = new DateTime(1, 1, 1, 0, 0, 0);
            if (_dpExpire.DateTime > new DateTime(1753, 1, 1) && _dpExpire.DateTime < new DateTime(9999, 12, 31))
                _document.ExpireDate = _dpExpire.DateTime;
            else
                _document.ExpireDate = new DateTime(1, 1, 1, 0, 0, 0);

            // Update default template
            if (_ddlDefaultTemplate.SelectedIndex > 0)
            {
                _document.Template = int.Parse(_ddlDefaultTemplate.SelectedValue);
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
            foreach (var item in _cControl.DataTypes)
            {
                _document.getProperty(item.Key).Value = item.Value.Data.Value;
            }

            // Update the update date
            _dp.Text = _document.UpdateDate.ToShortDateString() + " " + _document.UpdateDate.ToShortTimeString();
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
            if (!base.ValidateUserApp(Constants.Applications.Content))
            {
                ShowUserValidationError("<h3>The current user doesn't have access to this application</h3><p>Please contact the system administrator if you think that you should have access.</p>");
                return false;
            }
            if (!ValidateUserNodeTreePermissions(_document.Path, ActionBrowse.Instance.Letter.ToString()))
            {
                ShowUserValidationError(
                    "<h3>The current user doesn't have permissions to browse this document</h3><p>Please contact the system administrator if you think that you should have access.</p>");
                return false;
            }
            //TODO: Change this, when we add view capabilities, the user will be able to view but not edit!
            if (!ValidateUserNodeTreePermissions(_document.Path, ActionUpdate.Instance.Letter.ToString()))
            {
                ShowUserValidationError("<h3>The current user doesn't have permissions to edit this document</h3><p>Please contact the system administrator if you think that you should have access.</p>");
                return false;
            }
            return true;
        }

        private void AddPreviewButton(uicontrols.ScrollingMenu menu, int id)
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

            if (EnablePreviewButton())
            {
                menuItem.AltText = ui.Text("buttons", "showPage", UmbracoUser);
                menuItem.OnClickCommand = "window.open('dialogs/preview.aspx?id=" + id + "','umbPreview')";
            }
            else
            {
                var showPageDisabledText = ui.Text("buttons", "showPageDisabled", UmbracoUser);
                if (showPageDisabledText.StartsWith("["))
                    showPageDisabledText = ui.GetText("buttons", "showPageDisabled", null, "en");

                menuItem.AltText = showPageDisabledText;
                ((Image) menuItem).Attributes.Add("style", "opacity: 0.5");
            }
        }

        private bool EnablePreviewButton()
        {
            // Fix for U4-862, if there's no template, disable the preview button
            // Fixed again for U4-2587, apparently at some point "no template" changed from -1 to 0? -SJ
            // Now also catches when template doesn't exist any more or is not allowed any more
            // Don't think there's a better way to check if the template exists besides trying to instantiate it..
            try
            {
                var template = new businesslogic.template.Template(_document.Template);
                // If template is found check if it's in the list of allowed templates for this document
                return _document.Content.ContentType.AllowedTemplates.ToList().Any(t => t.Id == template.Id);
            }
            catch (Exception) { }
            
            return false;
        }

        /// <summary>
        /// JsInclude1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

        /// <summary>
        /// JsInclude2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude2;

        /// <summary>
        /// JsInclude3 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude3;

        /// <summary>
        /// plc control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder plc;

        /// <summary>
        /// doSave control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlInputHidden doSave;

        /// <summary>
        /// doPublish control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlInputHidden doPublish;

    }
}
