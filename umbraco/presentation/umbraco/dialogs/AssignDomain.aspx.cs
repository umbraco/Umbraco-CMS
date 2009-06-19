using System;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.web;

namespace umbraco.dialogs
{
    /// <summary>
    /// Summary description for AssignDomain.
    /// </summary>
    public partial class AssignDomain : UmbracoEnsuredPage
    {
        private int currentID;
        private int editDomain;

        protected void Page_Load(object sender, EventArgs e)
        {
            currentID = int.Parse(helper.Request("id"));
            prop_domain.Text = umbraco.ui.Text("assignDomain", "domain", this.getUser());
            prop_lang.Text = umbraco.ui.Text("general", "language", this.getUser());
            pane_addnew.Text = umbraco.ui.Text("assignDomain", "addNew", this.getUser());
            pane_edit.Text = umbraco.ui.Text("assignDomain", "orEdit", this.getUser());
            
            // Put user code to initialize the page here
            if (helper.Request("editDomain").Trim() != "")
            {
                editDomain = int.Parse(helper.Request("editDomain"));
            }

            if (helper.Request("delDomain").Trim() != "")
            {
                Domain d = new Domain(int.Parse(helper.Request("delDomain")));
                FeedBackMessage.type = umbraco.uicontrols.Feedback.feedbacktype.success;
                FeedBackMessage.Text = ui.Text("assignDomain", "domainDeleted", d.Name, base.getUser());
                d.Delete();
                updateDomainList();
            }

            if (!IsPostBack)
            {
                // Add caption to button
                ok.Text = ui.Text("assignDomain", "addNew", base.getUser());
              

                int selectedLanguage = -1;

                // Maybe add editing info - not the best way this is made ;-)
                if (editDomain > 0)
                {
                    Domain d = new Domain(editDomain);
                    selectedLanguage = d.Language.id;
                    DomainName.Text = d.Name;
                    ok.Text = ui.Text("general", "update", base.getUser());
                }

                // Add caption to language validator
                LanguageValidator.ErrorMessage = ui.Text("defaultdialogs", "requiredField", base.getUser()) + "<br/>";
                DomainValidator.ErrorMessage = ui.Text("defaultdialogs", "requiredField", base.getUser());

                Languages.Items.Add(new ListItem(ui.Text("general", "choose", base.getUser()), ""));
                foreach (Language l in Language.getAll)
                {
                    ListItem li = new ListItem();
                    li.Text = l.FriendlyName;
                    li.Value = l.id.ToString();
                    if (selectedLanguage == l.id)
                        li.Selected = true;
                    Languages.Items.Add(li);
                }
            }

            updateDomainList();
        }

        private void updateDomainList()
        {
            
            Domain[] domainList = Domain.GetDomainsById(currentID);

            if (domainList.Length > 0) {
                allDomains.Text = "<table border=\"0\" cellspacing=\"10\">";

                foreach (Domain d in domainList) {
                    allDomains.Text += "<tr><td>" + d.Name + "</td><td><a href=\"?id=" + currentID + "&editDomain=" +
                                       d.Id.ToString() + "\">" + ui.Text("edit") + "</a></td><td><a href=\"?id=" + currentID +
                                       "&delDomain=" + d.Id.ToString() + "\" onClick=\"return confirm('" +
                                       ui.Text("defaultdialogs", "confirmdelete", base.getUser()) +
                                       "');\" style=\"color: red\">" + ui.Text("delete") + "</a></td></tr>";
                }

                allDomains.Text += "</table>";
                pane_edit.Visible = true;                 
            } else {
                pane_edit.Visible = false;
            }


        }

        protected void SaveDomain(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                if (editDomain > 0)
                {
                    Domain d = new Domain(editDomain);
                    d.Language = new Language(int.Parse(Languages.SelectedValue));
                    d.Name = DomainName.Text.ToLower();
                    FeedBackMessage.type = umbraco.uicontrols.Feedback.feedbacktype.success;
                    FeedBackMessage.Text = ui.Text("assignDomain", "domainUpdated", DomainName.Text, base.getUser());
                    d.Save();

                    DomainName.Text = "";
                    Languages.SelectedIndex = 0;
                    updateDomainList();

                    //this is probably the worst webform I've ever seen... 
                    Response.Redirect("AssignDomain.aspx?id=" + currentID.ToString());
                }
                else
                {
                    
                  
                    if( !Domain.Exists(DomainName.Text.ToLower())) {
                        Domain.MakeNew(DomainName.Text, currentID, int.Parse(Languages.SelectedValue));
                        FeedBackMessage.Text = ui.Text("assignDomain", "domainCreated", DomainName.Text, base.getUser());
                        FeedBackMessage.type = umbraco.uicontrols.Feedback.feedbacktype.success;

                        DomainName.Text = "";
                        Languages.SelectedIndex = 0;
                        updateDomainList();

                        //this is probably the worst webform I've ever seen... 
                        Response.Redirect("AssignDomain.aspx?id=" + currentID.ToString());
                    } else {
                        FeedBackMessage.Text = ui.Text("assignDomain", "domainExists", DomainName.Text, base.getUser());
                        FeedBackMessage.type = umbraco.uicontrols.Feedback.feedbacktype.error;
                    }
                }

                
            }
        }

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
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
    }
}