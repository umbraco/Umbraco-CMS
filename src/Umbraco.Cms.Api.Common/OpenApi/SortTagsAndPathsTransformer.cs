using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Transforms the OpenAPI document to sort tags and paths alphabetically.
/// </summary>
internal class SortTagsAndPathsTransformer : IOpenApiDocumentTransformer
{
    /// <summary>
    /// Transforms the specified OpenAPI document to sort its tags and paths alphabetically.
    /// </summary>
    /// <param name="document">The <see cref="OpenApiDocument"/> to modify.</param>
    /// <param name="context">The <see cref="OpenApiDocumentTransformerContext"/> associated with the <paramref name="document"/>.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Tags = new SortedSet<OpenApiTag>(
            document.Tags ?? Enumerable.Empty<OpenApiTag>(),
            Comparer<OpenApiTag>.Create((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal)));

        var sortedPaths = new OpenApiPaths();
        foreach (KeyValuePair<string, IOpenApiPathItem> keyValuePair in document.Paths
                     .OrderBy(x => x.Value.Operations?.FirstOrDefault().Value?.Tags?.FirstOrDefault()?.Name)
                     .ThenBy(x => x.Key))
        {
            sortedPaths.Add(keyValuePair.Key, keyValuePair.Value);
        }

        document.Paths = sortedPaths;
        return Task.CompletedTask;
    }
}
