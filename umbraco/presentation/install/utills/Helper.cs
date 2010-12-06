using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace umbraco.presentation.install {
  public class Helper {

    public static void RedirectToNextStep(Page page) {
      _default d = (_default)page;
      d.GotoNextStep(d.step.Value);
    }

  }
}