using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using umbraco.cms.businesslogic.media;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for Media
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/webservices")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]

    public class MediaPickerService : System.Web.Services.WebService
    {

        [WebMethod]
        [ScriptMethod]
        public string GetThumbNail(int mediaId)
        {
            return MediaPickerServiceHelpers.GetThumbNail(mediaId);
                        
        }

        [WebMethod]
        [ScriptMethod]
        public  string GetFile(int mediaId)
        {
            return MediaPickerServiceHelpers.GetFile(mediaId);

        }
    }
}
