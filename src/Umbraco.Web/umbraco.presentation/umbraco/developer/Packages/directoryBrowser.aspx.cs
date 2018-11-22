using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using umbraco.BusinessLogic;

namespace umbraco.presentation.developer.packages
{
    [Obsolete("This class is no longer used and will be removed from the codebase in the future")]
    public partial class directoryBrowser : BasePages.UmbracoEnsuredPage
    {
        public directoryBrowser()
        {
            CurrentApp = DefaultApps.developer.ToString();
        }

        /// <summary>
        /// pane control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane;
    }
}
