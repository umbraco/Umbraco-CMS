using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Snippets
{
    /// <summary>
    /// The collection of partial view macro snippets.
    /// </summary>
    public class PartialViewMacroSnippetCollection : BuilderCollectionBase<ISnippet>
    {
        public PartialViewMacroSnippetCollection(Func<IEnumerable<ISnippet>> items) : base(items)
        {
        }

        /// <summary>
        /// Gets the partial view macro snippet names.
        /// </summary>
        /// <returns>The names of all partial view macro snippets.</returns>
        public IEnumerable<string> GetNames()
        {
            var snippetNames = this.Select(x => Path.GetFileNameWithoutExtension(x.Name)).ToArray();

            // Ensure the ones that are called 'Empty' are at the top
            var empty = snippetNames.Where(x => Path.GetFileName(x)?.InvariantStartsWith("Empty") ?? false)
                .OrderBy(x => x?.Length).ToArray();

            return empty.Union(snippetNames.Except(empty)).WhereNotNull();
        }

        /// <summary>
        /// Gets the content of a partial view macro snippet as a string.
        /// </summary>
        /// <param name="snippetName">The name of the snippet.</param>
        /// <returns>The content of the partial view macro.</returns>
        public string GetContentFromName(string snippetName)
        {
            if (snippetName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(snippetName));
            }

            string partialViewMacroHeader = "@inherits Umbraco.Cms.Web.Common.Macros.PartialViewMacroPage";

            var snippet = this.Where(x => x.Name.Equals(snippetName + ".cshtml")).FirstOrDefault();

            // Try and get the snippet path
            if (snippet is null)
            {
                throw new InvalidOperationException("Could not load snippet with name " + snippetName);
            }

            // Strip the @inherits if it's there
            var snippetContent = StripPartialViewHeader(snippet.Content);

            var content = $"{partialViewMacroHeader}{Environment.NewLine}{snippetContent}";
            return content;
        }

        private string StripPartialViewHeader(string contents)
        {
            var headerMatch = new Regex("^@inherits\\s+?.*$", RegexOptions.Multiline);
            return headerMatch.Replace(contents, string.Empty);
        }
    }
}
