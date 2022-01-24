namespace Umbraco.Cms.Core.Security
{
    public class NoOpHtmlSanitizer : IHtmlSanitizer
    {
        public string Sanitize(string html)
        {
            return html;
        }
    }
}
