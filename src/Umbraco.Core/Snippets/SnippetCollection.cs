using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Snippets
{
    /// <summary>
    /// The collection of partial view macro snippets.
    /// </summary>
    public class SnippetCollection : BuilderCollectionBase<ISnippet>
    {
        public SnippetCollection(Func<IEnumerable<ISnippet>> items) : base(items)
        {
        }

        /// <summary>
        /// Gets the partial view macro snippet names.
        /// </summary>
        /// <returns>The name of the partial view macro snippets.</returns>
        public IEnumerable<string> GetPartialViewMacroSnippetNames() => GetSnippetNames();

        /// <summary>
        /// Gets the partial view snippet names.
        /// </summary>
        /// <returns>The name of the partial view snippets.</returns>
        public IEnumerable<string> GetPartialViewSnippetNames()
        {
            // Ignore these
            var filterNames = new string[]
            {
                "Gallery",
                "ListChildPagesFromChangeableSource",
                "ListChildPagesOrderedByProperty",
                "ListImagesFromMediaFolder"
            };

            return GetSnippetNames(filterNames);
        }

        /// <summary>
        /// Gets the content of a partial view snippet as a string.
        /// </summary>
        /// <param name="snippetName">The name of the snippet.</param>
        /// <returns>The content of the partial view.</returns>
        public string GetPartialViewSnippetContent(string snippetName) => GetPartialViewMacroSnippetContent(snippetName, PartialViewType.PartialView);

        /// <summary>
        /// Gets the content of a macro partial view snippet as a string.
        /// </summary>
        /// <param name="snippetName">The name of the snippet.</param>
        /// <returns>The content of the partial view macro.</returns>
        public string GetPartialViewMacroSnippetContent(string snippetName) => GetPartialViewMacroSnippetContent(snippetName, PartialViewType.PartialViewMacro);

        private string GetPartialViewMacroSnippetContent(string snippetName, PartialViewType partialViewType)
        {
            if (snippetName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(snippetName));
            }

            string partialViewHeader;
            switch (partialViewType)
            {
                case PartialViewType.PartialView:
                    partialViewHeader = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage";
                    break;
                case PartialViewType.PartialViewMacro:
                    partialViewHeader = "@inherits Umbraco.Cms.Web.Common.Macros.PartialViewMacroPage";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(partialViewType));
            }

            var snippet = this.Where(x => x.Name.Equals(snippetName + ".cshtml")).FirstOrDefault();

            // Try and get the snippet path
            if (snippet is null)
            {
                throw new InvalidOperationException("Could not load snippet with name " + snippetName);
            }

            // Strip the @inherits if it's there
            var snippetContent = StripPartialViewHeader(snippet.Content);

            // Update Model.Content to be Model when used as PartialView
            if (partialViewType == PartialViewType.PartialView)
            {
                snippetContent = snippetContent
                    .Replace("Model.Content.", "Model.")
                    .Replace("(Model.Content)", "(Model)");
            }

            var content = $"{partialViewHeader}{Environment.NewLine}{snippetContent}";
            return content;
        }

        private IEnumerable<string> GetSnippetNames(params string[] filterNames)
        {
            var files = this.Select(x => Path.GetFileNameWithoutExtension(x.Name))
                .Except(filterNames, StringComparer.InvariantCultureIgnoreCase)
                .ToArray();

            // Ensure the ones that are called 'Empty' are at the top
            var empty = files.Where(x => Path.GetFileName(x)?.InvariantStartsWith("Empty") ?? false)
                .OrderBy(x => x?.Length)
                .ToArray();

            return empty.Union(files.Except(empty)).WhereNotNull();
        }

        private string StripPartialViewHeader(string contents)
        {
            var headerMatch = new Regex("^@inherits\\s+?.*$", RegexOptions.Multiline);
            return headerMatch.Replace(contents, string.Empty);
        }
    }
}
