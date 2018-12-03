using Umbraco.Core.Services;
using System;
using Umbraco.Core;

namespace umbraco.presentation.umbraco.dialogs {
    public partial class insertMasterpagePlaceholder : Umbraco.Web.UI.Pages.UmbracoEnsuredPage {

        public insertMasterpagePlaceholder()
        {
            CurrentApp = Constants.Applications.Settings.ToString();
        }
        protected void Page_Load(object sender, EventArgs e) {
            //labels
            pp_placeholder.Text = Services.TextService.Localize("placeHolderID");

        }
    }
}
