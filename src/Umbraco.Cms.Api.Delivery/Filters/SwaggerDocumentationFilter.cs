using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Umbraco.Cms.Api.Delivery.Filters;

[Obsolete($"Superseded by {nameof(SwaggerContentDocumentationFilter)} and {nameof(SwaggerMediaDocumentationFilter)}. Will be removed in V14.")]
public class SwaggerDocumentationFilter : IOperationFilter, IParameterFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // retained for backwards compat
    }

    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        // retained for backwards compat
    }
}
