namespace Umbraco.Core.Security
{
   public interface IMarkdownSanitizer
   {
       /// <summary>
       /// Sanitizes Markdown
       /// </summary>
       /// <param name="markdown">Markdown to be sanitized</param>
       /// <returns>Sanitized Markdown</returns>
       string Sanitize(string markdown);
   }
}


