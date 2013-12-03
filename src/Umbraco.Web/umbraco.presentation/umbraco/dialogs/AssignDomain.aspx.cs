using System;
using System.Globalization;
using System.Web.UI.WebControls;
using Umbraco.Web;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.web;

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
            CurrentApp = DefaultApps.content.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _currentId = Request.GetItemAs<int>("id");
            prop_domain.Text = ui.Text("assignDomain", "domain", UmbracoUser);
            prop_lang.Text = ui.Text("general", "language", UmbracoUser);
            pane_addnew.Text = ui.Text("assignDomain", "addNew", UmbracoUser);
            pane_edit.Text = ui.Text("assignDomain", "orEdit", UmbracoUser);

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
                ok.Text = ui.Text("assignDomain", "addNew", UmbracoUser);

                var selectedLanguage = -1;

                // Maybe add editing info - not the best way this is made ;-)
                if (_editDomain > 0)
                {
                    var d = new Domain(_editDomain);
                    selectedLanguage = d.Language.id;
                    DomainName.Text = d.Name.StartsWith("*") ? "*" : d.Name;
                    ok.Text = ui.Text("general", "update", UmbracoUser);
                }

                // Add caption to language validator
                LanguageValidator.ErrorMessage = ui.Text("defaultdialogs", "requiredField", UmbracoUser) + "<br/>";
                DomainValidator.ErrorMessage = ui.Text("defaultdialogs", "requiredField", UmbracoUser);

                DomainValidator2.ErrorMessage = ui.Text("assignDomain", "invalidDomain", UmbracoUser);
                //DomainValidator2.ValidationExpression = @"^(?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/[-\w]*)?$";
                DomainValidator2.ValidationExpression = @"^(\*|((?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/[-\w]*)?))$";

                Languages.Items.Add(new ListItem(ui.Text("general", "choose", UmbracoUser), ""));
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
                                       d.Id.ToString(CultureInfo.InvariantCulture) + "\">" + ui.Text("edit") + "</a></td><td><a href=\"?id=" + _currentId +
                                       "&delDomain=" + d.Id.ToString(CultureInfo.InvariantCulture) + "\" onClick=\"return confirm('" +
                                       ui.Text("defaultdialogs", "confirmdelete", UmbracoUser) +
                                       "');\" style=\"color: red\">" + ui.Text("delete") + "</a></td></tr>";
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