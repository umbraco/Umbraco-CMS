using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.OpenApi;

internal class NotificationHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.HttpMethod == HttpMethod.Get.Method)
        {
            return;
        }

        // filter out irrelevant responses (401 will never produce notifications)
        IEnumerable<OpenApiResponse> relevantResponses = operation
            .Responses
            .Where(pair => pair.Key != StatusCodes.Status401Unauthorized.ToString())
            .Select(pair => pair.Value);
        foreach (OpenApiResponse response in relevantResponses)
        {
            response.Headers.TryAdd(Constants.Headers.Notifications, new OpenApiHeader
            {
                Description = "The list of notifications produced during the request.",
                Schema = new OpenApiSchema { Type = "array" }
            });
        }
    }
}
