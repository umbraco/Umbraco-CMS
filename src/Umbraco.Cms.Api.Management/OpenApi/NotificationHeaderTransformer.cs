using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.OpenApi;

/// <summary>
/// Transforms OpenAPI operations to include notification headers in responses.
/// </summary>
internal sealed class NotificationHeaderTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc/>
    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.HttpMethod == HttpMethod.Get.Method)
        {
            return;
        }

        Type notificationHeaderModelType = typeof(NotificationHeaderModel);
        OpenApiSchema notificationHeaderSchema = await context.GetOrCreateSchemaAsync(
            notificationHeaderModelType,
            cancellationToken: cancellationToken);
        context.Document?.AddComponent(notificationHeaderModelType.Name, notificationHeaderSchema);

        // filter out irrelevant responses (401 will never produce notifications)
        IEnumerable<OpenApiResponse> relevantResponses = operation
                                                             .Responses
                                                             ?.Where(pair =>
                                                                 pair.Key !=
                                                                 StatusCodes.Status401Unauthorized.ToString())
                                                             .Select(pair => pair.Value)
                                                             .OfType<OpenApiResponse>()
                                                         ?? [];
        foreach (OpenApiResponse response in relevantResponses)
        {
            response.Headers ??= new Dictionary<string, IOpenApiHeader>();
            response.Headers.TryAdd(
                Constants.Headers.Notifications,
                new OpenApiHeader
                {
                    Description = "The list of notifications produced during the request.",
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array | JsonSchemaType.Null,
                        Items = new OpenApiSchemaReference(notificationHeaderModelType.Name, context.Document)
                    }
                });
        }
    }
}
