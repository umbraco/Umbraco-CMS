using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace umbraco.uicontrols
{
    public class ProgressBar : System.Web.UI.WebControls.Image
    {
        private string _title = umbraco.ui.Text("publish", "inProgress", null);
        public string Title { get; set; }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if(!string.IsNullOrEmpty(Title))
                _title = Title;

            base.ImageUrl = IO.SystemDirectories.Umbraco_client + "/images/progressBar.gif";
            base.AlternateText = _title;

            base.Render(writer);
        }
    }
}
