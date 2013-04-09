using System;
using System.Web.UI.WebControls;
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
            _currentId = int.Parse(helper.Request("id"));
            prop_domain.Text = ui.Text("assignDomain", "domain", this.getUser());
            prop_lang.Text = ui.Text("general", "language", this.getUser());
            pane_addnew.Text = ui.Text("assignDomain", "addNew", this.getUser());
            pane_edit.Text = ui.Text("assignDomain", "orEdit", this.getUser());
            
            // Put user code to initialize the page here
            if (helper.Request("editDomain").Trim() != "")
            {
                _editDomain = int.Parse(helper.Request("editDomain"));
            }

            if (helper.Request("delDomain").Trim() != "")
            {
                var d = new Domain(int.Parse(helper.Request("delDomain")));
                FeedBackMessage.type = uicontrols.Feedback.feedbacktype.success;
                FeedBackMessage.Text = ui.Text("assignDomain", "domainDeleted", d.Name, getUser());
                d.Delete();
                UpdateDomainList();
            }

            if (!IsPostBack)
            {
                // Add caption to button
                ok.Text = ui.Text("assignDomain", "addNew", getUser());
              
                var selectedLanguage = -1;

                // Maybe add editing info - not the best way this is made ;-)
                if (_editDomain > 0)
                {
                    var d = new Domain(_editDomain);
                    selectedLanguage = d.Language.id;
					DomainName.Text = d.Name.StartsWith("*") ? "*" : d.Name;
                    ok.Text = ui.Text("general", "update", getUser());
                }

                // Add caption to language validator
                LanguageValidator.ErrorMessage = ui.Text("defaultdialogs", "requiredField", base.getUser()) + "<br/>";
                DomainValidator.ErrorMessage = ui.Text("defaultdialogs", "requiredField", base.getUser());

				DomainValidator2.ErrorMessage = ui.Text("assignDomain", "invalidDomain", base.getUser());
				//DomainValidator2.ValidationExpression = @"^(?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/[-\w]*)?$";
				DomainValidator2.ValidationExpression = @"^(\*|((?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/[-\w]*)?))$";

                Languages.Items.Add(new ListItem(ui.Text("general", "choose", base.getUser()), ""));
                foreach (var l in Language.getAll)
                {
                    var li = new ListItem();
                    li.Text = l.FriendlyName;
                    li.Value = l.id.ToString();
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
                                       d.Id.ToString() + "\">" + ui.Text("edit") + "</a></td><td><a href=\"?id=" + _currentId +
                                       "&delDomain=" + d.Id.ToString() + "\" onClick=\"return confirm('" +
                                       ui.Text("defaultdialogs", "confirmdelete", base.getUser()) +
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
                    FeedBackMessage.Text = ui.Text("assignDomain", "domainUpdated", DomainName.Text, getUser());
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
					domainName = domainName == "*" ? ("*" + _currentId.ToString()) : domainName;

                    if (!Domain.Exists(domainName.ToLower()))
                    {
                        Domain.MakeNew(domainName, _currentId, int.Parse(Languages.SelectedValue));
                        FeedBackMessage.Text = ui.Text("assignDomain", "domainCreated", domainName, getUser());
                        FeedBackMessage.type = uicontrols.Feedback.feedbacktype.success;

                        DomainName.Text = "";
                        Languages.SelectedIndex = 0;
                        UpdateDomainList();

                        //this is probably the worst webform I've ever seen... 
                        Response.Redirect("AssignDomain.aspx?id=" + _currentId.ToString());
                    }
                    else
                    {
                        FeedBackMessage.Text = ui.Text("assignDomain", "domainExists", DomainName.Text.Trim(), getUser());
                        FeedBackMessage.type = uicontrols.Feedback.feedbacktype.error;
                    }
                }   
            }
        }

    }
}