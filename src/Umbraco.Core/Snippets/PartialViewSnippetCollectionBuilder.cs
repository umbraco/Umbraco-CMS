using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Snippets;

/// <summary>
/// The partial view snippet collection builder.
/// </summary>
public partial class PartialViewSnippetCollectionBuilder : LazyCollectionBuilderBase<PartialViewSnippetCollectionBuilder, PartialViewSnippetCollection, PartialViewSnippet>
{
    protected override PartialViewSnippetCollectionBuilder This => this;

    protected override IEnumerable<PartialViewSnippet> CreateItems(IServiceProvider factory)
    {
        var embeddedSnippets = new List<PartialViewSnippet>(base.CreateItems(factory));

        var snippetProvider = new EmbeddedFileProvider(typeof(IAssemblyProvider).Assembly, "Umbraco.Cms.Core.EmbeddedResources.Snippets");
        IEnumerable<IFileInfo> embeddedFiles = snippetProvider.GetDirectoryContents(string.Empty)
            .Where(x => !x.IsDirectory && x.Name.EndsWith(".cshtml"));

        IShortStringHelper shortStringHelper = factory.GetRequiredService<IShortStringHelper>();
        foreach (IFileInfo file in embeddedFiles)
        {
            var id = Path.GetFileNameWithoutExtension(file.Name);
            var name = id.SplitPascalCasing(shortStringHelper).ToFirstUpperInvariant();
            using var stream = new StreamReader(file.CreateReadStream());
            var content = CleanUpSnippetContent(stream.ReadToEnd().Trim());
            embeddedSnippets.Add(new PartialViewSnippet(id, name, content));
        }

        return embeddedSnippets;
    }

    private string CleanUpSnippetContent(string content)
    {
        const string partialViewHeader = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage";
        content = content.EnsureNativeLineEndings();

        // Strip the @inherits if it's there
        Regex headerMatch = HeaderRegex();
        var newContent = headerMatch.Replace(content, string.Empty)
            .Replace("Model.Content.", "Model.")
            .Replace("(Model.Content)", "(Model)")
            .Replace("Model?.Content.", "Model.")
            .Replace("(Model?.Content)", "(Model)");

        return $"{partialViewHeader}{Environment.NewLine}{newContent}";
    }

    [GeneratedRegex("^@inherits\\s+?.*$", RegexOptions.Multiline)]
    private static partial Regex HeaderRegex();
}
