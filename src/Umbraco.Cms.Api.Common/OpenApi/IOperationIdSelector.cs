using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Umbraco.Cms.Api.Common.OpenApi;

public interface IOperationIdSelector
{
    [Obsolete("Use overload that only takes ApiDescription instead. This will be removed in Umbraco 15.")]
    string? OperationId(ApiDescription apiDescription, ApiVersioningOptions apiVersioningOptions);

    string? OperationId(ApiDescription apiDescription);
}
