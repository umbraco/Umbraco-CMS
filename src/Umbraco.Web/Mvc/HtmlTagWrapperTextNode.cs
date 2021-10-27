using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
    public class HtmlTagWrapperTextNode : IHtmlTagWrapper
    {
        public string Content { get; private set; }
        public HtmlTagWrapperTextNode(string content)
        {
            this.Content = content;
        }

        public void WriteToHtmlTextWriter(System.Web.UI.HtmlTextWriter html)
        {
            html.WriteEncodedText(Content);
        }
    }
}
