using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.UI.Install.Steps
{
    public partial class RenderingEngine : StepUserControl
    {
        protected override void GotoNextStep(object sender, EventArgs e)
        {
            ////set the default engine
            //UmbracoSettings.DefaultRenderingEngine = Core.RenderingEngine.Mvc;
            //UmbracoSettings.Save();

            base.GotoNextStep(sender, e);
        }
    }
}