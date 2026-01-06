using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Api.Management.OpenApi.Transformers;

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

        // filter out irrelevant responses (401 will never produce notifications)
        List<OpenApiResponse> relevantResponses = operation
             .Responses
             ?.Where(pair =>
                 pair.Key !=
                 StatusCodes.Status401Unauthorized.ToString())
             .Select(pair => pair.Value)
             .OfType<OpenApiResponse>()
             .ToList()
             ?? [];

        if (relevantResponses.Count == 0)
        {
            return;
        }

        Type eventMessageType = typeof(EventMessageType);
        var eventMessageTypeSchemaId = UmbracoSchemaIdGenerator.Generate(eventMessageType);
        if (context.Document?.Components?.Schemas?.ContainsKey(eventMessageTypeSchemaId) != true)
        {
            // Ensure the EventMessageType schema is registered and doesn't get inlined
            OpenApiSchema eventMessageTypeSchema = await context.GetOrCreateSchemaAsync(
                eventMessageType,
                cancellationToken: cancellationToken);
            context.Document?.AddComponent(eventMessageTypeSchemaId, eventMessageTypeSchema);
        }

        Type notificationHeaderModelType = typeof(NotificationHeaderModel);
        var notificationHeaderSchemaId = UmbracoSchemaIdGenerator.Generate(notificationHeaderModelType);
        if (context.Document?.Components?.Schemas?.ContainsKey(notificationHeaderSchemaId) != true)
        {
            OpenApiSchema notificationHeaderSchema = await context.GetOrCreateSchemaAsync(
                notificationHeaderModelType,
                cancellationToken: cancellationToken);
            notificationHeaderSchema.Properties ??= new Dictionary<string, IOpenApiSchema>();
            notificationHeaderSchema.Properties["type"] = new OpenApiSchemaReference(eventMessageTypeSchemaId, context.Document);
            context.Document?.AddComponent(notificationHeaderSchemaId, notificationHeaderSchema);
        }

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
                        Items = new OpenApiSchemaReference(notificationHeaderSchemaId, context.Document),
                    },
                });
        }
    }
}
