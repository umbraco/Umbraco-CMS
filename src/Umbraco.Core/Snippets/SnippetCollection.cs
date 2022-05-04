using Umbraco.Cms.Core.Composing;
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

        public IEnumerable<string> GetPartialViewMacroSnippetNames() => GetSnippetNames();

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
    }
}
