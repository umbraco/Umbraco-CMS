namespace Umbraco.Core.Security
{
    public class NoopMarkdownSanitizer : IMarkdownSanitizer
    {
        public string Sanitize(string markdown)
        {
            return markdown;
        }
    }
}
