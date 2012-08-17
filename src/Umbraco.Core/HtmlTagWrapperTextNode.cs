namespace Umbraco.Core
{
    internal class HtmlTagWrapperTextNode : IHtmlTagWrapper
    {
		public string Content { get; set; }
        public HtmlTagWrapperTextNode(string content)
        {
            this.Content = content;
        }

        public void WriteToHtmlTextWriter(System.Web.UI.HtmlTextWriter html)
        {
            html.Write(Content);
        }
    }
}
