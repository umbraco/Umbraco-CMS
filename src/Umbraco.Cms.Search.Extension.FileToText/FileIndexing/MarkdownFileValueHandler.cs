using Umbraco.Cms.Search.Extension.FileToText.Extensions;
using Markdig;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Extension.FileToText.FileIndexing;

internal sealed class MarkdownFileValueHandler : IFileValueHandler, IDefaultFileValueHandler
{
    public bool CanHandle(string extension)
        => extension.InvariantEquals(".md");

    public async Task<string> GetFileContentsAsync(Stream stream, CancellationToken cancellationToken)
    {
        string fileContent;
        using(var reader = new StreamReader(stream))
        {
            fileContent = await reader.ReadToEndAsync(cancellationToken);
        }

        var text = Markdown.ToPlainText(fileContent);
        return text.IsNullOrWhiteSpace()
            ? string.Empty
            : text.TrimWhitespaceAndNewlines();
    }
}
