using System;
using System.Data;
using System.Threading;
using System.Web;
using System.Collections;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using umbraco.BasePages;
using umbraco.BusinessLogic.console;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for trashcan
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/webservices")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class trashcan : System.Web.Services.WebService
    {
        [WebMethod]
        public void EmptyTrashcan()
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                Application["trashcanEmptyLeft"] = RecycleBin.Count().ToString();
                emptyTrashCanDo();
            }

        }

        [WebMethod]
        public string GetTrashStatus()
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                if (Application["trashcanEmptyLeft"] != null)
                    return Application["trashcanEmptyLeft"].ToString();
                else
                    return "";
            }

            return "-";

        }

        private void emptyTrashCanDo()
        {
            RecycleBin trashCan = new RecycleBin(Document._objectType);
            foreach (IconI d in trashCan.Children)
            {
                new Document(d.Id).delete();
                Application.Lock();
                Application["trashcanEmptyLeft"] = RecycleBin.Count().ToString();
                Application.UnLock();
            }
        }
    }
}
