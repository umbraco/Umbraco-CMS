using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace umbraco.presentation.developer.packages {
    public partial class SubmitPackage : BasePages.UmbracoEnsuredPage {

        public SubmitPackage()
        {
            CurrentApp = BusinessLogic.DefaultApps.developer.ToString();

        }
        private cms.businesslogic.packager.PackageInstance pack;
        private cms.businesslogic.packager.CreatedPackage createdPackage;

        protected void Page_Load(object sender, EventArgs e) {

            if(!String.IsNullOrEmpty(helper.Request("id"))){

                if (!IsPostBack) {
                    dd_repositories.Items.Clear();

                    dd_repositories.Items.Add(new ListItem("Choose a repository...", ""));

                    List<cms.businesslogic.packager.repositories.Repository> repos = cms.businesslogic.packager.repositories.Repository.getAll();

                    if (repos.Count == 1) {
                        ListItem li = new ListItem(repos[0].Name, repos[0].Guid);
                        li.Selected = true;

                        dd_repositories.Items.Add(li);
                        
                        pl_repoChoose.Visible = false;
                        pl_repoLogin.Style.Clear();
                        
                        privateRepoHelp.Visible = false;
                        publicRepoHelp.Style.Clear();

                    } else if (repos.Count == 0) {
                        Response.Redirect("editpackage.aspx?id=" + helper.Request("id"));
                    } else {

                        foreach (cms.businesslogic.packager.repositories.Repository repo in repos) {
                            dd_repositories.Items.Add(new ListItem(repo.Name, repo.Guid));
                        }

                        dd_repositories.Items[0].Selected = true;

                        dd_repositories.Attributes.Add("onChange", "onRepoChange()");

                    }
                }

                createdPackage = cms.businesslogic.packager.CreatedPackage.GetById(int.Parse(helper.Request("id")));
                pack = createdPackage.Data;

                if (pack.Url != "") {
                    Panel1.Text = "Submit '" + pack.Name + "' to a repository";
                }
            }
        }

        protected void submitPackage(object sender, EventArgs e) {

            Page.Validate();
            string feedback = "";

            if (Page.IsValid) {

                try {
                    var repo = cms.businesslogic.packager.repositories.Repository.getByGuid(dd_repositories.SelectedValue);

                    if (repo == null)
                    {
                        throw new InvalidOperationException("Could not find repository with id " + dd_repositories.SelectedValue);
                    }
                    
                    var memberKey = repo.Webservice.authenticate(tb_email.Text, library.md5(tb_password.Text));

                    byte[] doc = new byte[0];

                    if (fu_doc.HasFile)
                        doc = fu_doc.FileBytes;



                    if (memberKey != "") {

                        string result = repo.SubmitPackage(memberKey, pack, doc).ToString().ToLower();

                        switch (result) {
                            case "complete":
                                feedback = "Your package has been submitted successfully. It will be reviewed by the package repository administrator before it's publicly available";
                                fb_feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
                                break;
                            case "error":
                                feedback = "There was a general error submitting your package to the repository. This can be due to general communitations error or too much traffic. Please try again later";
                                fb_feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                                break;
                            case "exists":
                                feedback = "This package has already been submitted to the repository. You cannot submit it again. If you have updates for a package, you should contact the repositor administrator to submit an update";
                                fb_feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                                break;
                            case "noaccess":
                                feedback = "Authentication failed, You do not have access to this repository. Contact your package repository administrator";
                                fb_feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                                break;
                            default:
                                break;
                        }

                        if (result == "complete") {
                            Pane1.Visible = false;
                            Pane2.Visible = false;
                            submitControls.Visible = false;
                            feedbackControls.Visible = true;
                        } else {
                            Pane1.Visible = true;
                            Pane2.Visible = true;
                            submitControls.Visible = true;
                            feedbackControls.Visible = false;
                        }

                    } else {
                        feedback = "Authentication failed, You do not have access to this repository. Contact your package repository administrator";
                        fb_feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                    }
                } catch {
                    feedback = "Authentication failed, or the repository is currently off-line. Contact your package repository administrator";
                    fb_feedback.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                }

                fb_feedback.Text = feedback;
            }



        }
    }
}
