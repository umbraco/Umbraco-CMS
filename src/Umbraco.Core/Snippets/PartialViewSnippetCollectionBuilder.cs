using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Extensions;

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
            var hostingEnvironment = factory.GetRequiredService<IHostEnvironment>();
            var embeddedSnippets = GetEmbeddedSnippets();

            var customSnippetsDir = new DirectoryInfo(hostingEnvironment.MapPathContentRoot($"{Constants.SystemDirectories.Umbraco}/PartialViewMacros/Templates"));
            if (!customSnippetsDir.Exists)
            {
                return embeddedSnippets;
            }

            var customSnippets = customSnippetsDir.GetFiles().Select(f => new Snippet(f.Name, File.ReadAllText(f.FullName)));
            var allSnippets = Merge(embeddedSnippets, customSnippets);

            return allSnippets;
        }

        private IEnumerable<ISnippet> GetEmbeddedSnippets()
        {
            var embeddedSnippets = new List<ISnippet>();

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

        private IEnumerable<ISnippet> Merge(IEnumerable<ISnippet> embeddedSnippets, IEnumerable<ISnippet> customSnippets)
        {
            var allSnippets = embeddedSnippets.Concat(customSnippets);

            var duplicates = allSnippets.GroupBy(s => s.Name)
                .Where(gr => gr.Count() > 1) // Groups the snippets with the same name
                .Select(s => s.First()); // Takes the first element from a grouping, which is the embeded snippet with that same name,
                                         // since the physical snippet files are placed after the embedded ones in the all snippets colleciton

            // Remove any embedded snippets if a physical file with the same name can be found
            return allSnippets.Except(duplicates);
        }
    }
}
