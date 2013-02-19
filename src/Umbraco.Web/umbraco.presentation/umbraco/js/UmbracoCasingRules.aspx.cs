using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Strings;
using umbraco.cms.helpers;

namespace umbraco.presentation.js
{
    public partial class UmbracoCasingRules : BasePages.UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "text/javascript";
            // defines the constants _and_ provides code for safeAlias(alias) and isSafeAlias(alias)
            Response.Write(ShortStringHelperResolver.Current.Helper.CleanStringForSafeAliasJavaScriptCode);
        }
    }
}
