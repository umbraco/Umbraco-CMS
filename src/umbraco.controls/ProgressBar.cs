using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;

namespace umbraco.uicontrols
{
    [Obsolete("Use Umbraco.Web.UI.Controls.ProgressBar")]
    public class ProgressBar : System.Web.UI.WebControls.Panel
    {
        private string _title = umbraco.ui.Text("publish", "inProgress");
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
