using Microsoft.Extensions.FileProviders;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Snippets
{
    /// <summary>
    /// The partial view snippet collection builder.
    /// </summary>
    public class PartialViewSnippetCollectionBuilder : LazyCollectionBuilderBase<PartialViewSnippetCollectionBuilder, PartialViewSnippetCollection, ISnippet>
    {
        protected override PartialViewSnippetCollectionBuilder This => this;

        protected override IEnumerable<ISnippet> CreateItems(IServiceProvider factory)
        {
            var embeddedSnippets = new List<ISnippet>(base.CreateItems(factory));

            // Ignore these
            var filterNames = new List<string>
            {
                "Gallery",
                "ListChildPagesFromChangeableSource",
                "ListChildPagesOrderedByProperty",
                "ListImagesFromMediaFolder"
            };

            var snippetProvider = new EmbeddedFileProvider(typeof(IAssemblyProvider).Assembly, "Umbraco.Cms.Core.EmbeddedResources.Snippets");
            var embeddedFiles = snippetProvider.GetDirectoryContents(string.Empty)
                                    .Where(x => !x.IsDirectory && x.Name.EndsWith(".cshtml"));

            foreach (var file in embeddedFiles)
            {
                if (!filterNames.Contains(Path.GetFileNameWithoutExtension(file.Name)))
                {
                    using var stream = new StreamReader(file.CreateReadStream());
                    embeddedSnippets.Add(new Snippet(file.Name, stream.ReadToEnd().Trim()));
                }
            }

            return embeddedSnippets;
        }
    }
}
