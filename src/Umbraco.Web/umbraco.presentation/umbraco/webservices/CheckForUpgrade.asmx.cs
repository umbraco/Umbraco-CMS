using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using Umbraco.Core;
using Umbraco.Web.WebServices;
using Umbraco.Core.Configuration;


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

            var check = new global::Umbraco.Web.org.umbraco.update.CheckForUpgrade();
            var result = check.CheckUpgrade(UmbracoVersion.Current.Major,
                                                                         UmbracoVersion.Current.Minor,
                                                                         UmbracoVersion.Current.Build,
                                                                         UmbracoVersion.CurrentComment);
            return new UpgradeResult(result.UpgradeType.ToString(), result.Comment, result.UpgradeUrl);
        }

        [WebMethod]
        [ScriptMethod]
        public void InstallStatus(bool isCompleted, string userAgent, string errorMsg)
        {
            bool isUpgrade = false;
            // if it's an upgrade, you'll need to be logged in before we allow this call
            if (!String.IsNullOrEmpty(global::Umbraco.Core.Configuration.GlobalSettings.ConfigurationStatus))
            {
                isUpgrade = true;
                try
                {
                    legacyAjaxCalls.Authorize();
                }
                catch (Exception)
                {
                    //we don't want to throw the exception back to JS
                    return;
                }
            }

            // Check for current install Id
            Guid installId = Guid.NewGuid();
            var installCookie = new BusinessLogic.StateHelper.Cookies.Cookie("umb_installId", 1);
            if (string.IsNullOrEmpty(installCookie.GetValue()) == false)
            {
                if (Guid.TryParse(installCookie.GetValue(), out installId))
                {
                    // check that it's a valid Guid
                    if (installId == Guid.Empty)
                        installId = Guid.NewGuid();

                }
            }
            installCookie.SetValue(installId.ToString());

            string dbProvider = string.Empty;
            if (string.IsNullOrEmpty(global::Umbraco.Core.Configuration.GlobalSettings.ConfigurationStatus) == false)
            dbProvider = ApplicationContext.Current.DatabaseContext.DatabaseProvider.ToString();

            var check = new global::Umbraco.Web.org.umbraco.update.CheckForUpgrade();
            check.Install(installId,
                isUpgrade,
                isCompleted,
                DateTime.Now,
                UmbracoVersion.Current.Major,
                UmbracoVersion.Current.Minor,
                UmbracoVersion.Current.Build,
                UmbracoVersion.CurrentComment,
                errorMsg,
                userAgent,
                dbProvider);
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
            UpgradeUrl = upgradeUrl + "?version=" + HttpContext.Current.Server.UrlEncode(UmbracoVersion.Current.ToString(3));
        }
    }
}
