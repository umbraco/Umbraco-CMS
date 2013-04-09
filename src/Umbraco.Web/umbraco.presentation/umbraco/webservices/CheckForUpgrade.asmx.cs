using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using Umbraco.Web.WebServices;


namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for CheckForUpgrade
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class CheckForUpgrade : UmbracoAuthorizedWebService
    {

        [WebMethod]
        [ScriptMethod]
        public UpgradeResult CallUpgradeService()
        {
            if (!AuthorizeRequest()) return null;
            var check = new org.umbraco.update.CheckForUpgrade();
            var result = check.CheckUpgrade(GlobalSettings.VersionMajor, GlobalSettings.VersionMinor, GlobalSettings.VersionPatch, GlobalSettings.VersionComment);
            return new UpgradeResult(result.UpgradeType.ToString(), result.Comment, result.UpgradeUrl);
        }

    }
    public class UpgradeResult
    {
        public string UpgradeType { get; set; }
        public string UpgradeComment { get; set; }
        public string UpgradeUrl { get; set; }

        public UpgradeResult() { }
        public UpgradeResult(string upgradeType, string upgradeComment, string upgradeUrl)
        {
            UpgradeType = upgradeType;
            UpgradeComment = upgradeComment;
            UpgradeUrl = upgradeUrl + "?version=" + HttpContext.Current.Server.UrlEncode(GlobalSettings.CurrentVersion);
        }
    }
}
