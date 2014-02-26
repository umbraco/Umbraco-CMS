using System;
using System.Web.UI;
using Umbraco.Web.Install;

namespace Umbraco.Web.UI.Install.Steps
{
    public abstract class StepUserControl : UserControl
    {
        protected string GetCurrentStep()
        {
            var defaultPage = (Default) Page;
            return defaultPage.step.Value;
        }

        //protected virtual void GotoNextStep(object sender, EventArgs e)
        //{
        //    InstallHelper.RedirectToNextStep(Page, GetCurrentStep());
        //}
    }
}