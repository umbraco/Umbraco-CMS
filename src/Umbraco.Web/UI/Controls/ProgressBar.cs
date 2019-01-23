using Umbraco.Core.IO;

namespace Umbraco.Web.UI.Controls
{
    public class ProgressBar : System.Web.UI.WebControls.Image
    {
        public string Title { get; set; }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            base.ImageUrl = "/images/progressBar.gif";
            base.AlternateText = Title;

            base.Render(writer);
        }
    }
}
