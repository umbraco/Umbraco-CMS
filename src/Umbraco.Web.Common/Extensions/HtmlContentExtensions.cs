using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Umbraco.Extensions;

public static class HtmlContentExtensions
{
    public static string ToHtmlString(this IHtmlContent content)
    {
        using (var writer = new StringWriter())
        {
            content.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }
    }
}
