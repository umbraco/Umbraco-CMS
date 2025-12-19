using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Transformer that tags operations based on their group name.
/// </summary>
internal class TagActionsByGroupNameTransformer : IOpenApiOperationTransformer, IOpenApiDocumentTransformer
{
    /// <summary>
    /// Transforms the specified OpenAPI operation in order to tag it by its group name.
    /// </summary>
    /// <param name="operation">The <see cref="OpenApiOperation"/> to modify.</param>
    /// <param name="context">The <see cref="OpenApiOperationTransformerContext"/> associated with the <paramref name="operation"/>.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Document is null || context.Description.GroupName is not { } groupName)
        {
            return Task.CompletedTask;
        }

        operation.Tags = new HashSet<OpenApiTagReference> { new(groupName) };
        if (context.Document.Tags?.Any(t => t.Name == groupName) == true)
        {
            return Task.CompletedTask;
        }

        context.Document.Tags ??= new HashSet<OpenApiTag>();
        context.Document.Tags.Add(new OpenApiTag { Name = groupName });
        return Task.CompletedTask;
    }

    /// <summary>
    /// Transforms the specified OpenAPI document in order to clean up unused tags.
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
        var usedTags = new HashSet<string?>(document.Paths
            .SelectMany(p => (p.Value.Operations ?? []).Values)
            .SelectMany(o => o.Tags ?? new HashSet<OpenApiTagReference>())
            .Select(t => t.Name));

        var tagsToRemove = (document.Tags ?? Enumerable.Empty<OpenApiTag>())
            .Where(tag => usedTags.Contains(tag.Name) is false)
            .ToList();

        foreach (OpenApiTag tag in tagsToRemove)
        {
            document.Tags?.Remove(tag);
        }

        return Task.CompletedTask;
    }
}
