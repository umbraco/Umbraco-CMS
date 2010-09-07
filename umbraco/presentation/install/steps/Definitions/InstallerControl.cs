using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace umbraco.presentation.install.steps.Definitions
{
    public class InstallerControl : System.Web.UI.UserControl
    {
        public void NextStep()
        {
            _default p = (_default)this.Page;
            p.GotoNextStep(helper.Request("installStep"));
        }
    }
}