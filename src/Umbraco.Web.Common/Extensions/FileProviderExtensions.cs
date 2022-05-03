using Microsoft.Extensions.FileProviders;

namespace Umbraco.Extensions;

internal static class FileProviderExtensions
{
    public static IFileProvider ConcatComposite(this IFileProvider fileProvider, params IFileProvider[] fileProviders)
    {
        IEnumerable<IFileProvider>? existingFileProviders = fileProvider switch
        {
            CompositeFileProvider compositeFileProvider => compositeFileProvider.FileProviders,
            _ => new[] { fileProvider },
        };

        return new CompositeFileProvider(existingFileProviders.Concat(fileProviders));
    }
}
