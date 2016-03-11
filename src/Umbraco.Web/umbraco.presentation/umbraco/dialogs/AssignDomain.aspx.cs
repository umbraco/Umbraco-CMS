using System;
using System.Globalization;
using System.Web.UI.WebControls;
using Umbraco.Web;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Web.UI.Pages;
using Umbraco.Core.Services;

namespace umbraco.dialogs
{
    /// <summary>
    /// Summary description for AssignDomain.
    /// </summary>
    public partial class AssignDomain : UmbracoEnsuredPage
    {
        private int _currentId;
        private int _editDomain;

        public AssignDomain()
        {
            CurrentApp = Constants.Applications.Content.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _currentId = Request.GetItemAs<int>("id");
            prop_domain.Text = Services.TextService.Localize("assignDomain/domain");
            prop_lang.Text = Services.TextService.Localize("general/language");
            pane_addnew.Text = Services.TextService.Localize("assignDomain/addNew");
            pane_edit.Text = Services.TextService.Localize("assignDomain/orEdit");

            if (Request.GetItemAsString("editDomain").Trim() != "")
            {
                _editDomain = Request.GetItemAs<int>("editDomain");
            }

            if (Request.GetItemAsString("delDomain").Trim() != "")
            {
                var d = new Domain(Request.GetItemAs<int>("delDomain"));
                FeedBackMessage.type = uicontrols.Feedback.feedbacktype.success;
                FeedBackMessage.Text = ui.Text("assignDomain", "domainDeleted", d.Name, UmbracoUser);
                d.Delete();
                UpdateDomainList();
            }

            if (!IsPostBack)
            {
                // Add caption to button
                ok.Text = Services.TextService.Localize("assignDomain/addNew");

                var selectedLanguage = -1;

                // Maybe add editing info - not the best way this is made ;-)
                if (_editDomain > 0)
                {
                    var d = new Domain(_editDomain);
                    selectedLanguage = d.Language.id;
                    DomainName.Text = d.Name.StartsWith("*") ? "*" : d.Name;
                    ok.Text = Services.TextService.Localize("general/update");
                }

                // Add caption to language validator
                LanguageValidator.ErrorMessage = Services.TextService.Localize("defaultdialogs/requiredField") + "<br/>";
                DomainValidator.ErrorMessage = Services.TextService.Localize("defaultdialogs/requiredField");

                DomainValidator2.ErrorMessage = Services.TextService.Localize("assignDomain/invalidDomain");
                //DomainValidator2.ValidationExpression = @"^(?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/[-\w]*)?$";
                DomainValidator2.ValidationExpression = @"^(\*|((?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/[-\w]*)?))$";

                Languages.Items.Add(new ListItem(Services.TextService.Localize("general/choose"), ""));
                foreach (var l in Language.GetAllAsList())
                {
                    var li = new ListItem();
                    li.Text = l.FriendlyName;
                    li.Value = l.id.ToString(CultureInfo.InvariantCulture);
                    if (selectedLanguage == l.id)
                        li.Selected = true;
                    Languages.Items.Add(li);
                }
            }

            UpdateDomainList();
        }

        private void UpdateDomainList()
        {

            var domainList = Domain.GetDomainsById(_currentId);

            if (domainList.Length > 0)
            {
                allDomains.Text = "<table border=\"0\" cellspacing=\"10\">";

                foreach (var d in domainList)
                {
                    var name = d.Name.StartsWith("*") ? "*" : d.Name;
                    allDomains.Text += "<tr><td>" + name + "</td><td>(" + d.Language.CultureAlias + ")</td><td><a href=\"?id=" + _currentId + "&editDomain=" +
                                       d.Id.ToString(CultureInfo.InvariantCulture) + "\">" + Services.TextService.Localize("edit") + "</a></td><td><a href=\"?id=" + _currentId +
                                       "&delDomain=" + d.Id.ToString(CultureInfo.InvariantCulture) + "\" onClick=\"return confirm('" +
                                       Services.TextService.Localize("defaultdialogs/confirmdelete") +
                                       "');\" style=\"color: red\">" + Services.TextService.Localize("delete") + "</a></td></tr>";
                }

                allDomains.Text += "</table>";
                pane_edit.Visible = true;
            }
            else
            {
                pane_edit.Visible = false;
            }
        }

        protected void SaveDomain(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                if (_editDomain > 0)
                {
                    var d = new Domain(_editDomain);
                    d.Language = new Language(int.Parse(Languages.SelectedValue));
                    d.Name = DomainName.Text.ToLower();
                    FeedBackMessage.type = uicontrols.Feedback.feedbacktype.success;
                    FeedBackMessage.Text = ui.Text("assignDomain", "domainUpdated", DomainName.Text, UmbracoUser);
                    d.Save();

                    DomainName.Text = "";
                    Languages.SelectedIndex = 0;
                    UpdateDomainList();

                    //this is probably the worst webform I've ever seen... 
                    Response.Redirect("AssignDomain.aspx?id=" + _currentId.ToString());
                }
                else
                {
                    // have to handle wildcard
                    var domainName = DomainName.Text.Trim();
                    domainName = domainName == "*" ? ("*" + _currentId.ToString(CultureInfo.InvariantCulture)) : domainName;

                    if (!Domain.Exists(domainName.ToLower()))
                    {
                        Domain.MakeNew(domainName, _currentId, int.Parse(Languages.SelectedValue));
                        FeedBackMessage.Text = ui.Text("assignDomain", "domainCreated", domainName, UmbracoUser);
                        FeedBackMessage.type = uicontrols.Feedback.feedbacktype.success;

                        DomainName.Text = "";
                        Languages.SelectedIndex = 0;
                        UpdateDomainList();

                        //this is probably the worst webform I've ever seen... 
                        Response.Redirect("AssignDomain.aspx?id=" + _currentId.ToString());
                    }
                    else
                    {
                        FeedBackMessage.Text = ui.Text("assignDomain", "domainExists", DomainName.Text.Trim(), UmbracoUser);
                        FeedBackMessage.type = uicontrols.Feedback.feedbacktype.error;
                    }
                }
            }
        }

        /// <summary>
        /// FeedBackMessage control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Feedback FeedBackMessage;

        /// <summary>
        /// pane_addnew control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_addnew;

        /// <summary>
        /// prop_domain control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel prop_domain;

        /// <summary>
        /// DomainName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox DomainName;

        /// <summary>
        /// DomainValidator control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RequiredFieldValidator DomainValidator;

        /// <summary>
        /// DomainValidator2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RegularExpressionValidator DomainValidator2;

        /// <summary>
        /// prop_lang control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel prop_lang;

        /// <summary>
        /// Languages control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DropDownList Languages;

        /// <summary>
        /// LanguageValidator control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RequiredFieldValidator LanguageValidator;

        /// <summary>
        /// ok control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button ok;

        /// <summary>
        /// pane_edit control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_edit;

        /// <summary>
        /// allDomains control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal allDomains;
    }
}