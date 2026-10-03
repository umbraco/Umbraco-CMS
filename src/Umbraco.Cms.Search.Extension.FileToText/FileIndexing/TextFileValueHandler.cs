using Umbraco.Cms.Search.Extension.FileToText.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Extension.FileToText.FileIndexing;

internal sealed class TextFileValueHandler : IFileValueHandler, IDefaultFileValueHandler
{
    public bool CanHandle(string extension)
        => extension.InvariantEquals(".txt");

    public async Task<string> GetFileContentsAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync(cancellationToken);
        return text.TrimWhitespaceAndNewlines();
    }
}
