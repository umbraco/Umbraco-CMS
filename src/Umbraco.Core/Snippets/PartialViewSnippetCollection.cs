using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Snippets
{
    /// <summary>
    /// The collection of partial view snippets.
    /// </summary>
    public class PartialViewSnippetCollection : BuilderCollectionBase<ISnippet>
    {
        public PartialViewSnippetCollection(Func<IEnumerable<ISnippet>> items) : base(items)
        {
        }

        /// <summary>
        /// Gets the partial view snippet names.
        /// </summary>
        /// <returns>The names of all partial view snippets.</returns>
        public IEnumerable<string> GetNames()
        {
            var snippetNames = this.Select(x => Path.GetFileNameWithoutExtension(x.Name)).ToArray();

            // Ensure the ones that are called 'Empty' are at the top
            var empty = snippetNames.Where(x => Path.GetFileName(x)?.InvariantStartsWith("Empty") ?? false)
                .OrderBy(x => x?.Length).ToArray();

            return empty.Union(snippetNames.Except(empty)).WhereNotNull();
        }

        /// <summary>
        /// Gets the content of a partial view snippet as a string.
        /// </summary>
        /// <param name="snippetName">The name of the snippet.</param>
        /// <returns>The content of the partial view.</returns>
        public string GetContentFromName(string snippetName)
        {
            if (snippetName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(snippetName));
            }

            string partialViewHeader = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage";

            var snippet = this.Where(x => x.Name.Equals(snippetName + ".cshtml")).FirstOrDefault();

            // Try and get the snippet path
            if (snippet is null)
            {
                throw new InvalidOperationException("Could not load snippet with name " + snippetName);
            }

            var snippetContent = CleanUpContents(snippet.Content);

            var content = $"{partialViewHeader}{Environment.NewLine}{snippetContent}";
            return content;
        }

        private static string CleanUpContents(string content)
        {
            // Strip the @inherits if it's there
            var headerMatch = new Regex("^@inherits\\s+?.*$", RegexOptions.Multiline);
            var newContent = headerMatch.Replace(content, string.Empty);

            return newContent
                .Replace("Model.Content.", "Model.")
                .Replace("(Model.Content)", "(Model)")
                .Replace("Model?.Content.", "Model.")
                .Replace("(Model?.Content)", "(Model)");
        }
    }
}
