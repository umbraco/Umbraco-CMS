namespace Umbraco.Core.Security
{
    public class NoOpHtmlSanitizer : IHtmlSanitizer
    {
        public string Sanitize(string html)
        {
            return html;
        }
    }
}
