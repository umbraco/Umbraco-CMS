using System;
using System.Web.Script.Services;
using System.Web.Services;
using System.ComponentModel;
using Umbraco.Web.WebServices;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for trashcan
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/webservices")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class trashcan : UmbracoAuthorizedWebService
    {
        [WebMethod]
        public void EmptyTrashcan(RecycleBin.RecycleBinType type)
        {
            //validate against the app type!
            switch (type)
            {
                case RecycleBin.RecycleBinType.Content:
                    if (!AuthorizeRequest(DefaultApps.content.ToString())) return;
                    break;
                case RecycleBin.RecycleBinType.Media:
                    if (!AuthorizeRequest(DefaultApps.media.ToString())) return;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            //TODO: This will never work in LB scenarios
            Application["trashcanEmptyLeft"] = RecycleBin.Count(type).ToString();
            emptyTrashCanDo(type);
        }

        [WebMethod]
        public string GetTrashStatus()
        {
            //TODO: This will never work in LB scenarios

            if (AuthorizeRequest())
            {
                return Application["trashcanEmptyLeft"] != null 
                    ? Application["trashcanEmptyLeft"].ToString() 
                    : "";
            }

            return "-";

        }

        private void emptyTrashCanDo(RecycleBin.RecycleBinType type)
        {
            var trashCan = new RecycleBin(type);

            var callback = new Action<int>(x =>
            {
                Application.Lock();
                Application["trashcanEmptyLeft"] = x.ToString();
                Application.UnLock();
            });

            trashCan.CallTheGarbageMan(callback);
            
        }
    }
}
