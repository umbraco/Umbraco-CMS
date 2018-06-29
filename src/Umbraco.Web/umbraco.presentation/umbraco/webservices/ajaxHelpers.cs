using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Umbraco.Core.IO;

namespace umbraco.presentation.webservices
{
    public class ajaxHelpers
    {
        public static void EnsureLegacyCalls(Page page)
        {
            var sm = ScriptManager.GetCurrent(page);
            var legacyPath = new ServiceReference(SystemDirectories.WebServices + "/legacyAjaxCalls.asmx");

            if (!sm.Services.Contains(legacyPath))
                sm.Services.Add(legacyPath);
        }
    }
}
