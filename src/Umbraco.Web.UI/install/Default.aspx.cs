using System;
using System.Web;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using Umbraco.Web.Install;
using Umbraco.Web.Security;
using Umbraco.Web.UI.Pages;
using umbraco;

namespace Umbraco.Web.UI.Install
{
	public partial class Default : BasePage
	{
        private string _installStep = "";

        protected string CurrentStepClass = "";

        protected void Page_Load(object sender, System.EventArgs e)
        {
            rp_steps.DataSource = InstallHelper.InstallerSteps.Values;
            rp_steps.DataBind();
        }

        private void LoadContent(InstallerStep currentStep)
        {
            PlaceHolderStep.Controls.Clear();
            PlaceHolderStep.Controls.Add(LoadControl(IOHelper.ResolveUrl(currentStep.UserControl)));
            step.Value = currentStep.Alias;
            CurrentStepClass = currentStep.Alias;
        }

        int _stepCounter = 0;
        protected void BindStep(object sender, RepeaterItemEventArgs e)
        {

            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                var i = (InstallerStep)e.Item.DataItem;

                if (!i.HideFromNavigation)
                {
                    var _class = (Literal)e.Item.FindControl("lt_class");
                    var name = (Literal)e.Item.FindControl("lt_name");

                    if (i.Alias == CurrentStepClass)
                        _class.Text = "active";

                    _stepCounter++;
                    name.Text = (_stepCounter).ToString() + " - " + i.Name;
                }
                else
                    e.Item.Visible = false;
            }
        }

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _installStep = Request.GetItemAsString("installStep");

            //if this is not an upgrade we will log in with the default user.
            // It's not considered an upgrade if the ConfigurationStatus is missing or empty.
            if (string.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus) == false)
            {
                var result = Security.ValidateCurrentUser(new HttpContextWrapper(Context));
                
                if (result == ValidateRequestAttempt.FailedTimedOut || result == ValidateRequestAttempt.FailedNoPrivileges)
                {
                    Response.Redirect(SystemDirectories.Umbraco + "/logout.aspx?redir=" + Server.UrlEncode(Request.RawUrl));
                }
            }

            var s = string.IsNullOrEmpty(_installStep)
                        ? InstallHelper.InstallerSteps["welcome"]
                        : InstallHelper.InstallerSteps[_installStep];

            LoadContent(s);
        }
        
	}
}