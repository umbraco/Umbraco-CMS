using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace umbraco.presentation.webservices {
    public class ajaxHelpers {
        public static void EnsureLegacyCalls(System.Web.UI.Page page) {
            ScriptManager sm = ScriptManager.GetCurrent(page);
            ServiceReference legacyPath = new ServiceReference(GlobalSettings.Path + "/webservices/legacyAjaxCalls.asmx");

            if (!sm.Services.Contains(legacyPath))
                sm.Services.Add(legacyPath);
        }
    }
}
