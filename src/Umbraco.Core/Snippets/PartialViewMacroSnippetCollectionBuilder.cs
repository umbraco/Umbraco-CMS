using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Extensions;

namespace Umbraco.Cms.Core.Snippets
{
    /// <summary>
    /// The partial view macro snippet collection builder.
    /// </summary>
    public class PartialViewMacroSnippetCollectionBuilder : LazyCollectionBuilderBase<PartialViewMacroSnippetCollectionBuilder, PartialViewMacroSnippetCollection, ISnippet>
    {
        protected override PartialViewMacroSnippetCollectionBuilder This => this;

        protected override IEnumerable<ISnippet> CreateItems(IServiceProvider factory)
        {
            var hostEnvironment = factory.GetRequiredService<IHostEnvironment>();

            var embeddedSnippets = new List<ISnippet>(base.CreateItems(factory));
            var snippetProvider = new EmbeddedFileProvider(typeof(IAssemblyProvider).Assembly, "Umbraco.Cms.Core.EmbeddedResources.Snippets");
            var embeddedFiles = snippetProvider.GetDirectoryContents(string.Empty)
                                    .Where(x => !x.IsDirectory && x.Name.EndsWith(".cshtml"));

            foreach (var file in embeddedFiles)
            {
                using var stream = new StreamReader(file.CreateReadStream());
                embeddedSnippets.Add(new Snippet(file.Name, stream.ReadToEnd().Trim()));
            }

            var customSnippetsDir = new DirectoryInfo(hostEnvironment.MapPathContentRoot($"{Constants.SystemDirectories.Umbraco}/PartialViewMacros/Templates"));
            if (!customSnippetsDir.Exists)
            {
                return embeddedSnippets;
            }

            var customSnippets = customSnippetsDir.GetFiles().Select(f => new Snippet(f.Name, File.ReadAllText(f.FullName)));
            var allSnippets = Merge(embeddedSnippets, customSnippets);

            return allSnippets;
        }

        private IEnumerable<ISnippet> Merge(IEnumerable<ISnippet> embeddedSnippets, IEnumerable<ISnippet> customSnippets)
        {
            var allSnippets = embeddedSnippets.Concat(customSnippets);

            var duplicates = allSnippets.GroupBy(s => s.Name)
                .Where(gr => gr.Count() > 1) // Finds the snippets with the same name
                .Select(s => s.First()); // Takes the first element from a grouping, which is the embeded snippet with that same name,
                                         // since the physical snippet files are placed after the embedded ones in the all snippets colleciton

            // Remove any embedded snippets if a physical file with the same name can be found
            return allSnippets.Except(duplicates);
        }
    }
}
