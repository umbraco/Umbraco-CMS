using Umbraco.Cms.Search.Extension.FileToText.ContentIndexing;
using Umbraco.Cms.Search.Extension.FileToText.FileIndexing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Search.Indexing;

namespace Umbraco.Cms.Search.Extension.FileToText.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileToText(this IServiceCollection services)
        => services
            .AddTransient<IContentIndexer, FilePropertyContentIndexer>()
            .AddTransient<IFileValueHandler, PdfFileValueHandler>()
            .AddTransient<IFileValueHandler, MarkdownFileValueHandler>()
            .AddTransient<IFileValueHandler, TextFileValueHandler>();
}
