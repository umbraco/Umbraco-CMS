using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.helpers;

namespace umbraco.presentation.js
{
    public partial class UmbracoCasingRules : BasePages.UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "text/javascript";
            Response.Write(String.Format(@"
var UMBRACO_FORCE_SAFE_ALIAS = {0};
var UMBRACO_FORCE_SAFE_ALIAS_VALIDCHARS = '{1}';
var UMBRACO_FORCE_SAFE_ALIAS_INVALID_FIRST_CHARS = '{2}';
", UmbracoSettings.ForceSafeAliases.ToString().ToLower(), Casing.VALID_ALIAS_CHARACTERS, Casing.INVALID_FIRST_CHARACTERS));

        }

       
    }
}
