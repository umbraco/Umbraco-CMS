namespace Umbraco.Core.Security
{
    public class NoopHtmlSanitizer : IHtmlSanitizer
    {
        public string Sanitize(string html)
        {
            return html;
        }
    }
}
