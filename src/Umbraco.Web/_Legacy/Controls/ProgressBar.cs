using System;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;

namespace Umbraco.Web._Legacy.Controls
{
    [Obsolete("Use Umbraco.Web.UI.Controls.ProgressBar")]
    public class ProgressBar : System.Web.UI.WebControls.Panel
    {
        private string _title = Current.Services.TextService.Localize("publish/inProgress");
        public string Title { get; set; }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if(!string.IsNullOrEmpty(Title))
                _title = Title;

            base.CssClass = "umb-loader";

            base.Render(writer);
        }
    }
}
