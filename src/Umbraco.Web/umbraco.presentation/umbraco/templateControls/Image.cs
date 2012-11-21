using System.Web.UI;
using System.Web.UI.HtmlControls;
using Umbraco.Core.Media;
using Umbraco.Web.Media;

namespace umbraco.presentation.templateControls
{
    public class Image : HtmlImage
    {
        public string NodeId { get; set; }
        public string Field { get; set; }
        public string Provider { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            int id;
            bool hasid = int.TryParse(NodeId, out id);
            int? nodeId = hasid ? id : (int?) null;
            
            Src = ImageUrl.GetImageUrl(Src, Field, Provider, nodeId);
            base.Render(writer);
        }
    }
}