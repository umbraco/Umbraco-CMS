namespace Umbraco.Cms.Core.Security;

public class NoopHtmlSanitizer : IHtmlSanitizer
{
    public string Sanitize(string html) => html;
}
