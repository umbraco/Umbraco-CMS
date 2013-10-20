using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

using Umbraco.Core.Configuration;
using Umbraco.Web.Models;

namespace Umbraco.Web.Editors
{
    public class UpdateCheckController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public UpgradeCheckResponse GetCheck()
        {
            //PP: The statehelper is obsolete, but there are NO directions on what to use instead, so keeping it here...
            var updChkCookie = new global::umbraco.BusinessLogic.StateHelper.Cookies.Cookie("UMB_UPDCHK", GlobalSettings.VersionCheckPeriod);
            string updateCheckCookie = updChkCookie.HasValue ? updChkCookie.GetValue() : "";

            if (GlobalSettings.VersionCheckPeriod > 0 && String.IsNullOrEmpty(updateCheckCookie) && Security.CurrentUser.UserType.Alias == "admin")
            {
                updChkCookie.SetValue("1");

                var check = new global::umbraco.presentation.org.umbraco.update.CheckForUpgrade();
                var result = check.CheckUpgrade(UmbracoVersion.Current.Major,
                                                                             UmbracoVersion.Current.Minor,
                                                                             UmbracoVersion.Current.Build,
                                                                             UmbracoVersion.CurrentComment);
                return new UpgradeCheckResponse(result.UpgradeType.ToString(), result.Comment, result.UpgradeUrl);
            }

            return null;
        }
    }
}
