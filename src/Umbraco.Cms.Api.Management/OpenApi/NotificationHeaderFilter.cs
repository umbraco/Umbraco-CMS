using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.OpenApi;

internal sealed class NotificationHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Only apply to the Umbraco CMS Management API.
        if (context.DocumentName != ManagementApiConfiguration.ApiName)
        {
            return;
        }

        if (context.ApiDescription.HttpMethod == HttpMethod.Get.Method)
        {
            return;
        }

        Type notificationModelType = typeof(NotificationHeaderModel);

        if (!context.SchemaRepository.Schemas.TryGetValue(notificationModelType.Name, out _))
        {
            context.SchemaGenerator.GenerateSchema(notificationModelType, context.SchemaRepository);
        }

        // filter out irrelevant responses (401 will never produce notifications)
        IEnumerable<OpenApiResponse> relevantResponses = operation
            .Responses?
            .Where(pair => pair.Key != StatusCodes.Status401Unauthorized.ToString())
            .Select(pair => pair.Value)
            .OfType<OpenApiResponse>()
            ?? Enumerable.Empty<OpenApiResponse>();
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
                        Items = new OpenApiSchemaReference(notificationModelType.Name),
                    },
                });
        }
    }
}
