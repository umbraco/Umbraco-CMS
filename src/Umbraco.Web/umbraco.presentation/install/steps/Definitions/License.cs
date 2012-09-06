using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.installer;

namespace umbraco.presentation.install.steps.Definitions {
  public class License : InstallerStep {
    public override string Alias {
      get { return "license"; }
    }

    public override string Name {
      get { return "License"; }
    }

    

    public override string UserControl {
      get { return IO.SystemDirectories.Install + "/steps/license.ascx"; }
    }

    public override bool Completed() {
      return false;
    }

   
  }
}