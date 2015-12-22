using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;

namespace umbraco.presentation.umbraco.dialogs {
    public partial class insertMasterpagePlaceholder : BasePages.UmbracoEnsuredPage {

        public insertMasterpagePlaceholder()
        {
            CurrentApp = Constants.Applications.Settings.ToString();
        }
        protected void Page_Load(object sender, EventArgs e) {
            //labels
            pp_placeholder.Text = ui.Text("placeHolderID");
           
        }
    }
}
