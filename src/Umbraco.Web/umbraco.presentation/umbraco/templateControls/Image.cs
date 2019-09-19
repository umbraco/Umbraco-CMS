using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Umbraco.Core.Media;
using Umbraco.Web.Media;

namespace umbraco.presentation.templateControls
{
    [Obsolete("This is no longer used and will be removed in future versions")]
    public class Image : HtmlImage
    {
        public string NodeId { get; set; }
        public string Field { get; set; }
        public string Provider { get; set; }
        public string Parameters { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            int id;
            bool hasid = int.TryParse(NodeId, out id);
            int? nodeId = hasid ? id : (int?)null;

            string url;
            bool imageFound = ImageUrl.TryGetImageUrl(Src, Field, Provider, Parameters, nodeId, out url);
            Src = url;
            if (imageFound)
            {
                base.Render(writer);
            }
        }
    }
}