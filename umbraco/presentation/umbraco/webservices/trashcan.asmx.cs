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
using umbraco.cms.businesslogic.media;

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
        public void EmptyTrashcan(cms.businesslogic.RecycleBin.RecycleBinType type)
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                Application["trashcanEmptyLeft"] = RecycleBin.Count(type).ToString();
                emptyTrashCanDo(type);
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

        private void emptyTrashCanDo(cms.businesslogic.RecycleBin.RecycleBinType type)
        {
            RecycleBin trashCan = new RecycleBin(type);
            //store children array here because iterating over an Array property object is very inneficient.
            var c = trashCan.Children;
            foreach (IconI d in c)
            {
                // we need this check as we can't move the responsibility to an interface
                if (type == RecycleBin.RecycleBinType.Content)
                {
                    new Document(d.Id).delete();
                }
                else if (type == RecycleBin.RecycleBinType.Media)
                {
                    new Media(d.Id).delete();
                }
                else
                {
                    throw new ArgumentException(String.Format("The type {0} is unknown in the emptyTrashCanDo method", type));
                }
                Application.Lock();
                Application["trashcanEmptyLeft"] = RecycleBin.Count(type).ToString();
                Application.UnLock();
            }
        }
    }
}
