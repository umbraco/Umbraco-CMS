using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Umbraco.Cms.Search.Extension.FileToText.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Extension.FileToText.FileIndexing;

internal sealed class PdfFileValueHandler : IFileValueHandler, IDefaultFileValueHandler
{
    public bool CanHandle(string extension)
        => extension.InvariantEquals(".pdf");

    public Task<string> GetFileContentsAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var document = PdfDocument.Open(stream);
        var text = new StringBuilder();
        foreach (Page page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            text.AppendLine(string.Join(" ", page.GetWords()));
        }

        return Task.FromResult(text.ToString().TrimWhitespaceAndNewlines());
    }
}
